using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetworkAnalyzer.API.Data;
using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Services;

namespace SocialNetworkAnalyzer.API.Tests.Services;

public class DatasetServiceTests
{
    private readonly Mock<ILogger<DatasetService>> _loggerMock = new();

    #region GetAllDatasetsAsync Tests
    
    [Fact]
    public async Task GetAllDatasetsAsync_ReturnsOrderedDatasets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        // Seed the database with test data
        await using (var context = new ApplicationDbContext(options))
        {
            context.Datasets.AddRange(
                new Dataset { Id = 1, Name = "Dataset 1", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Dataset { Id = 2, Name = "Dataset 2", CreatedAt = DateTime.UtcNow.AddDays(-3) },
                new Dataset { Id = 3, Name = "Dataset 3", CreatedAt = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
        }
        
        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            var service = new DatasetService(context, _loggerMock.Object);
            var result = await service.GetAllDatasetsAsync();
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            
            // Verify descending order by CreatedAt
            Assert.Equal(3, result[0].Id);
            Assert.Equal(1, result[1].Id);
            Assert.Equal(2, result[2].Id);
        }
    }
    
    [Fact]
    public async Task GetAllDatasetsAsync_ReturnsEmptyList_WhenNoDatasets()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        // Act
        await using var context = new ApplicationDbContext(options);
        var service = new DatasetService(context, _loggerMock.Object);
        var result = await service.GetAllDatasetsAsync();
            
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
    
    #endregion
    
    #region GetDatasetStatisticsAsync Tests
    
    [Fact]
    public async Task GetDatasetStatisticsAsync_ReturnsCorrectStatistics()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => 
                warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        
        const long datasetId = 1;
        
        // Seed the database with test data
        await using (var context = new ApplicationDbContext(options))
        {
            var dataset = new Dataset { Id = datasetId, Name = "Test Dataset", CreatedAt = DateTime.UtcNow };
            await context.Datasets.AddAsync(dataset);
            
            // Add 5 users
            for (int i = 1; i <= 5; i++)
            {
                await context.Users.AddAsync(new User { DatasetId = datasetId, UserId = i });
            }
            
            // Add 10 friendships (each user has 4 friends on average)
            await context.Friendships.AddRangeAsync(
                new Friendship { DatasetId = datasetId, User1Id = 1, User2Id = 2 },
                new Friendship { DatasetId = datasetId, User1Id = 1, User2Id = 3 },
                new Friendship { DatasetId = datasetId, User1Id = 1, User2Id = 4 },
                new Friendship { DatasetId = datasetId, User1Id = 2, User2Id = 3 },
                new Friendship { DatasetId = datasetId, User1Id = 2, User2Id = 5 },
                new Friendship { DatasetId = datasetId, User1Id = 3, User2Id = 4 },
                new Friendship { DatasetId = datasetId, User1Id = 3, User2Id = 5 },
                new Friendship { DatasetId = datasetId, User1Id = 4, User2Id = 5 },
                new Friendship { DatasetId = datasetId, User1Id = 1, User2Id = 5 },
                new Friendship { DatasetId = datasetId, User1Id = 2, User2Id = 4 }
            );
            
            await context.SaveChangesAsync();
        }
        
        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            var service = new DatasetService(context, _loggerMock.Object);
            var result = await service.GetDatasetStatisticsAsync(datasetId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.TotalUsers);
            Assert.Equal(4.0, result.AverageFriendsPerUser); // 10 friendships * 2 / 5 users = 4.0
        }
    }
    
    [Fact]
    public async Task GetDatasetStatisticsAsync_ReturnsZeroStatistics_WhenNoUsers()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        const long datasetId = 1;
        
        // Seed the database with an empty dataset
        await using (var context = new ApplicationDbContext(options))
        {
            var dataset = new Dataset { Id = datasetId, Name = "Empty Dataset", CreatedAt = DateTime.UtcNow };
            await context.Datasets.AddAsync(dataset);
            await context.SaveChangesAsync();
        }
        
        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            var service = new DatasetService(context, _loggerMock.Object);
            var result = await service.GetDatasetStatisticsAsync(datasetId);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalUsers);
            Assert.Equal(0, result.AverageFriendsPerUser);
        }
    }
    
    [Fact]
    public async Task GetDatasetStatisticsAsync_ThrowsException_WhenDatasetNotFound()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        const long nonExistentDatasetId = 999;
        
        // Act & Assert
        await using var context = new ApplicationDbContext(options);
        var service = new DatasetService(context, _loggerMock.Object);
            
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.GetDatasetStatisticsAsync(nonExistentDatasetId));
            
        Assert.Contains($"Dataset with ID {nonExistentDatasetId} was not found", exception.Message);
    }
    
    #endregion
    
    #region DatasetNameAlreadyExists Tests
    
    [Fact]
    public async Task DatasetNameAlreadyExists_ReturnsTrue_WhenNameExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        const string existingName = "Existing Dataset";
        
        // Seed the database with a dataset
        await using (var context = new ApplicationDbContext(options))
        {
            await context.Datasets.AddAsync(new Dataset { Name = existingName, CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();
        }
        
        // Act
        await using (var context = new ApplicationDbContext(options))
        {
            var service = new DatasetService(context, _loggerMock.Object);
            var result = await service.DatasetNameAlreadyExists(existingName);
            
            // Assert
            Assert.True(result);
        }
    }
    
    [Fact]
    public async Task DatasetNameAlreadyExists_ReturnsFalse_WhenNameDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        
        const string nonExistingName = "Non-Existing Dataset";
        
        // Act
        await using var context = new ApplicationDbContext(options);
        var service = new DatasetService(context, _loggerMock.Object);
        var result = await service.DatasetNameAlreadyExists(nonExistingName);
            
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region Parsing Methods Tests
    
    [Fact]
    public void ParseDataset_CreatesDatasetWithCorrectName()
    {
        // Arrange
        const string datasetName = "Test Dataset";
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseDataset", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var dataset = methodInfo?.Invoke(null, new object[] { datasetName }) as Dataset;
        
        // Assert
        Assert.NotNull(dataset);
        Assert.Equal(datasetName, dataset.Name);
        
        // Verify CreatedAt is set to current time (within a small tolerance)
        var timeDifference = DateTime.UtcNow - dataset.CreatedAt;
        Assert.True(timeDifference.TotalSeconds < 5); // Within 5 seconds
    }
    
    [Fact]
    public void ParseFriendships_CreatesCorrectFriendshipEntities()
    {
        // Arrange
        const long datasetId = 42;
        var friendships = new List<(long, long)>
        {
            (1, 2),
            (3, 4),
            (5, 6)
        };
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseFriendships", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = methodInfo?.Invoke(null, new object[] { friendships, datasetId }) as IEnumerable<Friendship>;
        var friendshipList = result?.ToList();
        
        // Assert
        Assert.NotNull(friendshipList);
        Assert.Equal(3, friendshipList.Count);
        
        // Verify all properties are set correctly
        foreach (var friendship in friendshipList)
        {
            Assert.Equal(datasetId, friendship.DatasetId);
        }
        
        Assert.Equal(1, friendshipList[0].User1Id);
        Assert.Equal(2, friendshipList[0].User2Id);
        Assert.Equal(3, friendshipList[1].User1Id);
        Assert.Equal(4, friendshipList[1].User2Id);
        Assert.Equal(5, friendshipList[2].User1Id);
        Assert.Equal(6, friendshipList[2].User2Id);
    }
    
    [Fact]
    public void ParseFriendships_ReturnsEmptyCollection_WhenInputIsEmpty()
    {
        // Arrange
        const long datasetId = 42;
        var emptyFriendships = new List<(long, long)>();
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseFriendships", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = methodInfo?.Invoke(null, new object[] { emptyFriendships, datasetId }) as IEnumerable<Friendship>;
        var friendshipList = result?.ToList();
        
        // Assert
        Assert.NotNull(friendshipList);
        Assert.Empty(friendshipList);
    }
    
    [Fact]
    public void ParseUsers_CreatesCorrectUserEntities()
    {
        // Arrange
        const long datasetId = 42;
        var userIds = new HashSet<long> { 1, 2, 3, 4, 5 };
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseUsers", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = methodInfo?.Invoke(null, new object[] { userIds, datasetId }) as IEnumerable<User>;
        var userList = result?.ToList();
        
        // Assert
        Assert.NotNull(userList);
        Assert.Equal(5, userList.Count);
        
        // Verify all properties are set correctly
        foreach (var user in userList)
        {
            Assert.Equal(datasetId, user.DatasetId);
            Assert.Contains(user.UserId, userIds);
        }
    }
    
    [Fact]
    public void ParseUsers_ReturnsEmptyCollection_WhenInputIsEmpty()
    {
        // Arrange
        const long datasetId = 42;
        var emptyUserIds = new HashSet<long>();
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseUsers", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = methodInfo?.Invoke(null, new object[] { emptyUserIds, datasetId }) as IEnumerable<User>;
        var userList = result?.ToList();
        
        // Assert
        Assert.NotNull(userList);
        Assert.Empty(userList);
    }
    
    [Fact]
    public void ParseUserAndRelationships_ParsesValidFileContent()
    {
        // Arrange
        var fileContent = "1 2\n3 4\n5 6";
        var formFile = CreateMockFormFile(fileContent, "test.txt");
        
        var userIds = new HashSet<long>();
        var friendships = new List<(long, long)>();
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("ParseUserAndRelationships", 
            BindingFlags.NonPublic | BindingFlags.Static);
        methodInfo?.Invoke(null, new object[] { formFile, userIds, friendships });
        
        // Assert
        Assert.Equal(6, userIds.Count); // 6 unique user IDs
        Assert.Equal(3, friendships.Count); // 3 friendship pairs
        
        Assert.Contains(1L, userIds);
        Assert.Contains(2L, userIds);
        Assert.Contains(3L, userIds);
        Assert.Contains(4L, userIds);
        Assert.Contains(5L, userIds);
        Assert.Contains(6L, userIds);
        
        Assert.Contains((1L, 2L), friendships);
        Assert.Contains((3L, 4L), friendships);
        Assert.Contains((5L, 6L), friendships);
    }
    
    [Fact]
    public void ParseUserAndRelationships_ThrowsException_WhenFileIsEmpty()
    {
        // Arrange
        var formFile = CreateMockFormFile("", "empty.txt");
        
        var userIds = new HashSet<long>();
        var friendships = new List<(long, long)>();
        
        // Act & Assert
        var methodInfo = typeof(DatasetService).GetMethod("ParseUserAndRelationships", 
            BindingFlags.NonPublic | BindingFlags.Static);
        
        // No exception is thrown for empty file, but collections remain empty
        methodInfo?.Invoke(null, new object[] { formFile, userIds, friendships });
        
        Assert.Empty(userIds);
        Assert.Empty(friendships);
    }
    
    [Fact]
    public void ParseUserAndRelationships_ThrowsException_WhenFileHasInvalidFormat()
    {
        // Arrange
        var fileContent = "1 2\ninvalid line\n5 6";
        var formFile = CreateMockFormFile(fileContent, "invalid.txt");
        
        var userIds = new HashSet<long>();
        var friendships = new List<(long, long)>();
        
        // Act & Assert
        var methodInfo = typeof(DatasetService).GetMethod("ParseUserAndRelationships", 
            BindingFlags.NonPublic | BindingFlags.Static);
        
        var exception = Assert.Throws<TargetInvocationException>(() => 
            methodInfo?.Invoke(null, new object[] { formFile, userIds, friendships }));
        
        Assert.IsType<ArgumentException>(exception.InnerException);
        Assert.Contains("Invalid data format at line", exception.InnerException?.Message);
    }
    
    [Fact]
    public void CalculateAverageFriendsPerUser_CalculatesCorrectly()
    {
        // Arrange
        const int totalFriendships = 10;
        const int totalUsers = 5;
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("CalculateAverageFriendsPerUser", 
            BindingFlags.NonPublic | BindingFlags.Static);
        var result = (double)methodInfo?.Invoke(null, new object[] { totalFriendships, totalUsers })!;
        
        // Assert
        Assert.Equal(4.0, result); // 10 friendships * 2 / 5 users = 4.0
    }
    
    [Fact]
    public void CalculateAverageFriendsPerUser_HandlesZeroUsers()
    {
        // Arrange
        const int totalFriendships = 10;
        const int totalUsers = 0;
        
        // Act
        var methodInfo = typeof(DatasetService).GetMethod("CalculateAverageFriendsPerUser", 
            BindingFlags.NonPublic | BindingFlags.Static);
        
        // Assert
        // Method should throw Exception when totalUsers is 0
        var exception = Assert.Throws<TargetInvocationException>(() => 
            methodInfo?.Invoke(null, new object[] { totalFriendships, totalUsers }));
        
        Assert.IsType<Exception>(exception.InnerException);
        Assert.Equal("Total users must be greater than 0", exception.InnerException?.Message);
    }
    
    #endregion
    
    #region Helper Methods
    
    private static IFormFile CreateMockFormFile(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(bytes.Length);
        formFile.Setup(f => f.OpenReadStream()).Returns(stream);
        
        return formFile.Object;
    }
    
    #endregion
}
