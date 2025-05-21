using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SocialNetworkAnalyzer.API.Data.Entities;

namespace SocialNetworkAnalyzer.API.Data.Configurations;

public class DatasetConfiguration : IEntityTypeConfiguration<Dataset>
{
    public void Configure(EntityTypeBuilder<Dataset> builder)
    {
        builder.Property(d => d.Name)
            .IsRequired();
        
        builder.Property(d => d.CreatedAt)
            .IsRequired();
            
        builder.HasMany(d => d.Users)
            .WithOne(u => u.Dataset)
            .HasForeignKey(u => u.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(d => d.Friendships)
            .WithOne(f => f.Dataset)
            .HasForeignKey(f => f.DatasetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
