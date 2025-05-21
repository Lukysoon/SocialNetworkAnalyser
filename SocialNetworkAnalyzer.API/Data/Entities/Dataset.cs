namespace SocialNetworkAnalyzer.API.Data.Entities;

public class Dataset
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
        
    public ICollection<User>? Users { get; set; }
    public ICollection<Friendship>? Friendships { get; set; }
}