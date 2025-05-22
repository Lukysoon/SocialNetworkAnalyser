namespace SocialNetworkAnalyzer.API.Models;

public struct DatasetGetDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}