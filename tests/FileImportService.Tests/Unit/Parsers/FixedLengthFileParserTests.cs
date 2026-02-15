using FileImportService.Application.Configuration;
using FileImportService.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FileImportService.Tests.Unit.Parsers;

public class FixedLengthFileParserTests
{
    private readonly Mock<ILogger<FixedLengthFileParser>> _loggerMock;
    private readonly Mock<IOptions<FileProcessingOptions>> _optionsMock;
    private readonly FixedLengthFileParser _parser;

    public FixedLengthFileParserTests()
    {
        _loggerMock = new Mock<ILogger<FixedLengthFileParser>>();
        _optionsMock = new Mock<IOptions<FileProcessingOptions>>();
        
        var options = new FileProcessingOptions
        {
            WatchFolder = "/tmp",
            ProcessedFolder = "/tmp/processed",
            ErrorFolder = "/tmp/error",
            FileTypes = new Dictionary<string, FileTypeConfiguration>
            {
                ["FixedLength"] = new FileTypeConfiguration
                {
                    ColumnDefinitions = new List<FixedLengthColumnDefinition>
                    {
                        new() { Name = "Field1", Start = 0, Length = 10 },
                        new() { Name = "Field2", Start = 10, Length = 20 },
                        new() { Name = "Field3", Start = 30, Length = 15 }
                    }
                }
            }
        };
        
        _optionsMock.Setup(x => x.Value).Returns(options);
        _parser = new FixedLengthFileParser(_optionsMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_ValidFixedLengthFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "SampleFiles", "sample-fixed.txt");

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ParsedRows.Should().HaveCount(5);
        result.ParsedRows[0].Values.Should().ContainKey("Field1");
        result.ParsedRows[0].Values.Should().ContainKey("Field2");
        result.ParsedRows[0].Values.Should().ContainKey("Field3");
        result.ParsedRows[0].Values["Field1"].Should().Be("0001");
        result.ParsedRows[0].Values["Field2"].Should().Be("John Doe");
    }

    [Fact]
    public async Task ParseAsync_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var filePath = "/tmp/nonexistent.txt";

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
