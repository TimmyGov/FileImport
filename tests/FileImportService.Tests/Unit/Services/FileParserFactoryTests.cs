using FileImportService.Application.Services;
using FileImportService.Domain.Enums;
using FluentAssertions;

namespace FileImportService.Tests.Unit.Services;

public class FileParserFactoryTests
{
    [Theory]
    [InlineData("test.csv", FileType.CSV)]
    [InlineData("test.xlsx", FileType.XLSX)]
    [InlineData("test.txt", FileType.TXT)]
    [InlineData("test-fixed.txt", FileType.FixedLength)]
    public void DetectFileType_ValidExtension_ReturnsCorrectType(string fileName, FileType expectedType)
    {
        // Act
        var result = FileParserFactory.DetectFileType(fileName);

        // Assert
        result.Should().Be(expectedType);
    }

    [Fact]
    public void DetectFileType_UnsupportedExtension_ThrowsException()
    {
        // Arrange
        var fileName = "test.doc";

        // Act & Assert
        var act = () => FileParserFactory.DetectFileType(fileName);
        act.Should().Throw<NotSupportedException>();
    }
}
