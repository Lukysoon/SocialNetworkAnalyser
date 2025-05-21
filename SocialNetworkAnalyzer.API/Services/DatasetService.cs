using System.Text;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using SocialNetworkAnalyzer.API.Data;
using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Models;
using SocialNetworkAnalyzer.API.ViewModels;

namespace SocialNetworkAnalyzer.API.Services
{
    public class DatasetService : IDatasetService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DatasetService> _logger;

        public DatasetService(ApplicationDbContext context, ILogger<DatasetService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<DatasetGetDto>> GetAllDatasetsAsync()
        {
            return await _context.Datasets
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DatasetGetDto(){Id = d.Id, Name = d.Name})
                .ToListAsync();
        }

        public async Task<Dataset> CreateDatasetAsync(string name, IFormFile file)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var dataset = ParseDataset(name);
                await _context.Datasets.AddAsync(dataset);
                await _context.SaveChangesAsync();

                var userIds = new HashSet<long>();
                var friendships = new List<(long, long)>();
                ParseUserAndRelationships(file, ref userIds, ref friendships);

                if (_context.Users.Any(u => userIds.Contains(u.Id)))
                    throw new ArgumentException(
                        "Cannot import dataset with an user ID which already exists in database.");

                if (userIds.Count == 0)
                    throw new ArgumentException("File contains no valid user data or friendships");

                var users = ParseUsers(userIds, dataset.Id);
                await _context.BulkInsertAsync(users);

                var friendshipEntities = ParseFriendships(friendships, dataset.Id);
                await _context.BulkInsertAsync(friendshipEntities);

                await _context.BulkSaveChangesAsync();
                await transaction.CommitAsync();

                return dataset;
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while creating dataset");
                throw new InvalidOperationException("Failed to save dataset to database", ex);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "IO error while reading dataset file");
                throw new ArgumentException($"Error reading dataset file: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating dataset");
                throw;
            }
        }
        
        private static Dataset ParseDataset(string name)
        {
            var dataset = new Dataset
            {
                Name = name,
                CreatedAt = DateTime.UtcNow
            };
            return dataset;
        }

        private static IEnumerable<Friendship> ParseFriendships(List<(long, long)> friendships, long datasetId)
        {
            var friendshipEntities = friendships.Select(f => new Friendship
            {
                DatasetId = datasetId,
                User1Id = f.Item1,
                User2Id = f.Item2
            });
            return friendshipEntities;
        }

        private static IEnumerable<User> ParseUsers(HashSet<long> userIds, long datasetId)
        {
            var users = userIds.Select(userId => new User
            {
                DatasetId = datasetId,
                UserId = userId
            });
            return users;
        }

        private static void ParseUserAndRelationships(IFormFile file, ref HashSet<long> userIds, ref List<(long, long)> friendships)
        {
            using var streamReader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, true, 512);
            int lineNumber = 0;

            while (streamReader.ReadLine() is { } line)
            {
                lineNumber++;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && 
                    long.TryParse(parts[0], out long user1Id) && 
                    long.TryParse(parts[1], out long user2Id))
                {
                    userIds.Add(user1Id);
                    userIds.Add(user2Id);
                            
                    friendships.Add((user1Id, user2Id));
                }
                else
                {
                    throw new ArgumentException($"Invalid data format at line {lineNumber}: '{line}'");
                }
            }
        }

        public async Task<DatasetStatistics> GetDatasetStatisticsAsync(long datasetId)
        {
            try
            {
                var datasetExists = await _context.Datasets.AnyAsync(d => d.Id == datasetId);
                if (!datasetExists)
                    throw new KeyNotFoundException($"Dataset with ID {datasetId} was not found.");

                var totalUsers = await GetTotalUsersCount(datasetId);

                if (totalUsers == 0)
                {
                    return new DatasetStatistics
                    {
                        TotalUsers = 0,
                        AverageFriendsPerUser = 0
                    };
                }

                var totalFriendships = await GetTotalFriendshipsForDataset(datasetId);
                var averageFriendsPerUser = CalculateAverageFriendsPerUser(totalFriendships, totalUsers);

                return new DatasetStatistics
                {
                    TotalUsers = totalUsers,
                    AverageFriendsPerUser = averageFriendsPerUser
                };
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for dataset {DatasetId}", datasetId);
                throw new InvalidOperationException($"Failed to retrieve statistics for dataset with ID {datasetId}", ex);
            }
        }

        public async Task<bool> DatasetNameAlreadyExists(string name)
        {
            return await _context.Datasets.AnyAsync(d => d.Name == name);
        }

        private async Task<int> GetTotalUsersCount(long datasetId)
        {
            return await _context.Users
                .Where(u => u.DatasetId == datasetId)
                .CountAsync();
        }

        private static double CalculateAverageFriendsPerUser(int totalFriendships, int totalUsers)
        {
            if (totalUsers == 0)
                throw new Exception("Total users must be greater than 0");
                
            return (double)totalFriendships * 2 / totalUsers;
        }

        private async Task<int> GetTotalFriendshipsForDataset(long datasetId)
        {
            return await _context.Friendships
                .Where(f => f.DatasetId == datasetId)
                .CountAsync();
        }
    }
}
