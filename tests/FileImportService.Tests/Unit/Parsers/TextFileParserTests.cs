using FileImportService.Infrastructure.Parsers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FileImportService.Tests.Unit.Parsers;

public class TextFileParserTests
{
    private readonly Mock<ILogger<TextFileParser>> _loggerMock;
    private readonly TextFileParser _parser;

    public TextFileParserTests()
    {
        _loggerMock = new Mock<ILogger<TextFileParser>>();
        _parser = new TextFileParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_ValidTextFile_ReturnsSuccess()
    {
        // Arrange
        var filePath = Path.Combine(AppContext.BaseDirectory, "SampleFiles", "sample.txt");

        // Act
        var result = await _parser.ParseAsync(filePath);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ParsedRows.Should().HaveCount(5);
        result.ParsedRows[0].Values.Should().ContainKey("Text");
        result.ParsedRows[0].Values["Text"].Should().Be("This is line 1");
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
