using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkAnalyzer.API.Data.Entities;

namespace SocialNetworkAnalyzer.API.Data.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.Property(f => f.DatasetId)
            .IsRequired();
            
        builder.Property(f => f.User1Id)
            .IsRequired();
            
        builder.Property(f => f.User2Id)
            .IsRequired();
            
        builder.HasIndex(f => new { f.DatasetId, f.User1Id, f.User2Id })
            .IsUnique();
            
        builder.HasOne(f => f.Dataset)
            .WithMany(d => d.Friendships)
            .HasForeignKey(f => f.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
