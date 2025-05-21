namespace SocialNetworkAnalyzer.API.Data.Entities;

public class User
{
    public long Id { get; set; }
    public long DatasetId { get; set; }
    public long UserId { get; set; }
        
    public Dataset? Dataset { get; set; }
}