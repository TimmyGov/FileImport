using FileImportService.Application.Services;
using FileImportService.Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileImportService.Tests.Unit.Services;

public class DataValidationServiceTests
{
    private readonly Mock<ILogger<DataValidationService>> _loggerMock;
    private readonly DataValidationService _service;

    public DataValidationServiceTests()
    {
        _loggerMock = new Mock<ILogger<DataValidationService>>();
        _service = new DataValidationService(_loggerMock.Object);
    }

    [Fact]
    public async Task ValidateDataAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var rows = new List<ParsedRow>
        {
            new() { RowNumber = 1, Values = new Dictionary<string, string> { ["Col1"] = "Value1" } },
            new() { RowNumber = 2, Values = new Dictionary<string, string> { ["Col1"] = "Value2" } }
        };

        // Act
        var result = await _service.ValidateDataAsync(rows);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateDataAsync_EmptyRows_ReturnsFailure()
    {
        // Arrange
        var rows = new List<ParsedRow>();

        // Act
        var result = await _service.ValidateDataAsync(rows);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("No data rows found"));
    }

    [Fact]
    public async Task ValidateDataAsync_RowWithNoData_ReturnsFailure()
    {
        // Arrange
        var rows = new List<ParsedRow>
        {
            new() { RowNumber = 1, Values = new Dictionary<string, string>() }
        };

        // Act
        var result = await _service.ValidateDataAsync(rows);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("has no data"));
    }
}
