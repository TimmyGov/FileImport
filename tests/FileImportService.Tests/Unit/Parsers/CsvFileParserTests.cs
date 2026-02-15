using FileImportService.Application.Configuration;
using FileImportService.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileImportService.Tests.Unit.Parsers;

public class CsvFileParserTests
{
    private readonly Mock<ILogger<CsvFileParser>> _loggerMock;
    private readonly Mock<IOptions<FileProcessingOptions>> _optionsMock;
    private readonly CsvFileParser _parser;

    public CsvFileParserTests()
    {
        _loggerMock = new Mock<ILogger<CsvFileParser>>();
        _optionsMock = new Mock<IOptions<FileProcessingOptions>>();
        
        var options = new FileProcessingOptions
        {
            WatchFolder = "/tmp",
            ProcessedFolder = "/tmp/processed",
            ErrorFolder = "/tmp/error",
            FileTypes = new Dictionary<string, FileTypeConfiguration>
            {
                ["CSV"] = new FileTypeConfiguration
                {
                    HasHeader = true,
                    Delimiter = ","
                }
            }
        };
        
        _optionsMock.Setup(x => x.Value).Returns(options);
        _parser = new CsvFileParser(_optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_ValidCsvFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "SampleFiles", "sample.csv");

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ParsedRows.Should().HaveCount(5);
        result.ParsedRows[0].Values.Should().ContainKey("Id");
        result.ParsedRows[0].Values.Should().ContainKey("Name");
        result.ParsedRows[0].Values["Id"].Should().Be("1");
        result.ParsedRows[0].Values["Name"].Should().Be("John Doe");
    }

    [Fact]
    public async Task ParseAsync_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var filePath = "/tmp/nonexistent.csv";

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
