using Microsoft.EntityFrameworkCore;
using SocialNetworkAnalyzer.API.Data.Configurations;
using SocialNetworkAnalyzer.API.Data.Entities;

namespace SocialNetworkAnalyzer.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Dataset> Datasets { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Friendship> Friendships { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DatasetConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FriendshipConfiguration());
    }
}
