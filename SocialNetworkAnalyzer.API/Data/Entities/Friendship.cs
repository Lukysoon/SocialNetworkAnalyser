namespace SocialNetworkAnalyzer.API.Data.Entities;

public class Friendship
{
    public long Id { get; set; }
    public long DatasetId { get; set; }
    public long User1Id { get; set; }
    public long User2Id { get; set; }
        
    public Dataset? Dataset { get; set; }
}