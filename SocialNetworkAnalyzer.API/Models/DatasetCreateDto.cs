namespace SocialNetworkAnalyzer.API.Models;

public class DatasetCreateDto
{
    public string Name { get; set; }
    public IFormFile file { get; set; }
}