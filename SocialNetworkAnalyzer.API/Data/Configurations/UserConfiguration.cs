using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkAnalyzer.API.Data.Entities;

namespace SocialNetworkAnalyzer.API.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.DatasetId)
            .IsRequired();
            
        builder.Property(u => u.UserId)
            .IsRequired();
            
        builder.HasIndex(u => new { u.DatasetId, u.UserId })
            .IsUnique();
            
        builder.HasOne(u => u.Dataset)
            .WithMany(d => d.Users)
            .HasForeignKey(u => u.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
