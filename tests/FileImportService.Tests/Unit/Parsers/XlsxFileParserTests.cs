using FileImportService.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileImportService.Tests.Unit.Parsers;

public class XlsxFileParserTests
{
    private readonly Mock<ILogger<XlsxFileParser>> _loggerMock;
    private readonly XlsxFileParser _parser;
    private readonly string _sampleFilePath;

    public XlsxFileParserTests()
    {
        _loggerMock = new Mock<ILogger<XlsxFileParser>>();
        _parser = new XlsxFileParser(_loggerMock.Object);
        
        // Generate sample Excel file
        _sampleFilePath = Path.Combine(AppContext.BaseDirectory, "SampleFiles", "sample.xlsx");
        Directory.CreateDirectory(Path.GetDirectoryName(_sampleFilePath)!);
        
        if (!File.Exists(_sampleFilePath))
        {
            SampleFiles.ExcelFileGenerator.GenerateSampleExcel(_sampleFilePath);
        }
    }

    [Fact]
    public async Task ParseAsync_ValidXlsxFile_ReturnsSuccess()
    {
        // Act
        var result = await _parser.ParseAsync(_sampleFilePath);

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
        var filePath = "/tmp/nonexistent.xlsx";

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }
}
