using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Models;

namespace SocialNetworkAnalyzer.API.Services;

public interface IDatasetService
{
    Task<List<DatasetGetDto>> GetAllDatasetsAsync();
    Task<Dataset> CreateDatasetAsync(string name, IFormFile file);
    Task<DatasetStatisticsDto> GetDatasetStatisticsAsync(long datasetId);
    Task<bool> DatasetNameAlreadyExists(string name);
}
