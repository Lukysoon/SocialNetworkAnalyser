using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SocialNetworkAnalyzer.API.Controllers;
using SocialNetworkAnalyzer.API.Data.Entities;
using SocialNetworkAnalyzer.API.Models;
using SocialNetworkAnalyzer.API.Services;
using System.Text;

namespace SocialNetworkAnalyzer.API.Tests.Controllers;

public class DatasetControllerTests
{
    private readonly Mock<IDatasetService> _datasetServiceMock = new();
    private readonly Mock<ILogger<DatasetController>> _loggerMock = new();
    private readonly DatasetController _controller;

    public DatasetControllerTests()
    {
        _controller = new DatasetController(_datasetServiceMock.Object, _loggerMock.Object);
    }

    #region GetAllDatasets Tests

    [Fact]
    public async Task GetAllDatasets_ReturnsOkResult_WithDatasets()
    {
        // Arrange
        var datasets = new List<DatasetGetDto>
        {
            new() { Id = 1, Name = "Dataset 1" },
            new() { Id = 2, Name = "Dataset 2" }
        };

        _datasetServiceMock.Setup(service => service.GetAllDatasetsAsync())
            .ReturnsAsync(datasets);

        // Act
        var result = await _controller.GetAllDatasets();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDatasets = Assert.IsAssignableFrom<IEnumerable<DatasetGetDto>>(okResult.Value);
        Assert.Equal(2, returnedDatasets.Count());
        Assert.Equal(datasets, returnedDatasets);
    }

    [Fact]
    public async Task GetAllDatasets_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _datasetServiceMock.Setup(service => service.GetAllDatasetsAsync())
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetAllDatasets();

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while retrieving datasets", statusCodeResult.Value);
        
        // Verify that the exception was logged
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region CreateDataset Tests

    [Fact]
    public async Task CreateDataset_ReturnsOkResult_WhenDatasetIsCreatedSuccessfully()
    {
        // Arrange
        var name = "Test Dataset";
        var file = CreateMockFormFile("1 2\n3 4", "test.txt");
        var createdDataset = new Dataset { Id = 1, Name = name, CreatedAt = DateTime.UtcNow };

        _datasetServiceMock.Setup(service => service.DatasetNameAlreadyExists(name))
            .ReturnsAsync(false);
        _datasetServiceMock.Setup(service => service.CreateDatasetAsync(name, file))
            .ReturnsAsync(createdDataset);

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDataset = Assert.IsType<Dataset>(okResult.Value);
        Assert.Equal(createdDataset.Id, returnedDataset.Id);
        Assert.Equal(createdDataset.Name, returnedDataset.Name);
    }

    [Fact]
    public async Task CreateDataset_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange
        var name = "";
        var file = CreateMockFormFile("1 2\n3 4", "test.txt");

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Dataset name is required", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateDataset_ReturnsBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var name = "Test Dataset";
        var file = CreateEmptyMockFormFile("test.txt");

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("File is required and cannot be empty", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateDataset_ReturnsBadRequest_WhenDatasetNameAlreadyExists()
    {
        // Arrange
        var name = "Existing Dataset";
        var file = CreateMockFormFile("1 2\n3 4", "test.txt");

        _datasetServiceMock.Setup(service => service.DatasetNameAlreadyExists(name))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Dataset with the same name already exists", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateDataset_ReturnsBadRequest_WhenFileExtensionIsNotTxt()
    {
        // Arrange
        var name = "Test Dataset";
        var file = CreateMockFormFile("1 2\n3 4", "test.csv");

        _datasetServiceMock.Setup(service => service.DatasetNameAlreadyExists(name))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Dataset must be a txt file", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateDataset_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var name = "Test Dataset";
        var file = CreateMockFormFile("1 2\n3 4", "test.txt");
        var exceptionMessage = "Test exception";

        _datasetServiceMock.Setup(service => service.DatasetNameAlreadyExists(name))
            .ReturnsAsync(false);
        _datasetServiceMock.Setup(service => service.CreateDatasetAsync(name, file))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _controller.CreateDataset(name, file);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal(exceptionMessage, statusCodeResult.Value);
        
        // Verify that the exception was logged
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region GetDatasetStatistics Tests

    [Fact]
    public async Task GetDatasetStatistics_ReturnsOkResult_WithStatistics()
    {
        // Arrange
        long datasetId = 1;
        var statistics = new DatasetStatisticsDto
        {
            TotalUsers = 5,
            AverageFriendsPerUser = 4.0
        };

        _datasetServiceMock.Setup(service => service.GetDatasetStatisticsAsync(datasetId))
            .ReturnsAsync(statistics);

        // Act
        var result = await _controller.GetDatasetStatistics(datasetId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStatistics = Assert.IsType<DatasetStatisticsDto>(okResult.Value);
        Assert.Equal(statistics.TotalUsers, returnedStatistics.TotalUsers);
        Assert.Equal(statistics.AverageFriendsPerUser, returnedStatistics.AverageFriendsPerUser);
    }

    [Fact]
    public async Task GetDatasetStatistics_ReturnsNotFound_WhenDatasetDoesNotExist()
    {
        // Arrange
        long datasetId = 999;
        var exceptionMessage = $"Dataset with ID {datasetId} was not found";

        _datasetServiceMock.Setup(service => service.GetDatasetStatisticsAsync(datasetId))
            .ThrowsAsync(new KeyNotFoundException(exceptionMessage));

        // Act
        var result = await _controller.GetDatasetStatistics(datasetId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(exceptionMessage, notFoundResult.Value);
        
        // Verify that the warning was logged
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDatasetStatistics_ReturnsInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        long datasetId = 1;

        _datasetServiceMock.Setup(service => service.GetDatasetStatisticsAsync(datasetId))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetDatasetStatistics(datasetId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Equal($"An error occurred while retrieving statistics for dataset with ID {datasetId}", statusCodeResult.Value);
        
        // Verify that the exception was logged
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFormFile(string content, string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(bytes.Length);
        formFile.Setup(f => f.OpenReadStream()).Returns(stream);
        
        return formFile.Object;
    }

    private static IFormFile CreateEmptyMockFormFile(string fileName)
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.Length).Returns(0);
        
        return formFile.Object;
    }

    #endregion
}
