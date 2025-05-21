using Microsoft.AspNetCore.Mvc;
using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Models;
using SocialNetworkAnalyzer.API.Services;

namespace SocialNetworkAnalyzer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatasetController : ControllerBase
    {
        private readonly IDatasetService _datasetService;
        private readonly ILogger<DatasetController> _logger;

        public DatasetController(IDatasetService datasetService, ILogger<DatasetController> logger)
        {
            _datasetService = datasetService;
            _logger = logger;
        }

        [HttpGet]
        [Route("/api/datasets")]
        public async Task<ActionResult<IEnumerable<DatasetGetDto>>> GetAllDatasets()
        {
            try
            {
                var datasets = await _datasetService.GetAllDatasetsAsync();
                return Ok(datasets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all datasets");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving datasets");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Dataset>> CreateDataset([FromForm] string name, [FromForm] IFormFile file)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest("Dataset name is required");

                if (file == null || file.Length == 0)
                    return BadRequest("File is required and cannot be empty");
                
                if (await _datasetService.DatasetNameAlreadyExists(name))
                    return BadRequest("Dataset with the same name already exists");
                
                if (file.FileName.Split('.').Length == 2 && file.FileName.Split('.')[1] != "txt")
                    return BadRequest("Dataset must be a txt file");

                var dataset = await _datasetService.CreateDatasetAsync(name, file);
                return Ok(dataset);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating dataset");
                return StatusCode(StatusCodes.Status500InternalServerError,ex.Message);
            }
        }

        [HttpGet("{id}/statistics")]
        public async Task<ActionResult<DatasetStatisticsDto>> GetDatasetStatistics(long id)
        {
            try
            {
                var statistics = await _datasetService.GetDatasetStatisticsAsync(id);
                return Ok(statistics);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Dataset not found");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics for dataset {DatasetId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while retrieving statistics for dataset with ID {id}");
            }
        }
    }
}
