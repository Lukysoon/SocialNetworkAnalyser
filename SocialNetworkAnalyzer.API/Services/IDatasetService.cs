using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Models;
using SocialNetworkAnalyzer.API.ViewModels;

namespace SocialNetworkAnalyzer.API.Services;

public interface IDatasetService
{
    Task<List<DatasetGetDto>> GetAllDatasetsAsync();
    Task<Dataset> CreateDatasetAsync(string name, IFormFile file);
    Task<DatasetStatistics> GetDatasetStatisticsAsync(long datasetId);
    Task<bool> DatasetNameAlreadyExists(string name);
}
