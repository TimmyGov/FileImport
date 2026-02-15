using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileImportService.Application.Services;

/// <summary>
/// Factory for creating appropriate file parser based on file type
/// </summary>
public class FileParserFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FileParserFactory> _logger;

    public FileParserFactory(
        IServiceProvider serviceProvider,
        ILogger<FileParserFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Get parser for specified file type
    /// </summary>
    /// <param name="fileType">Type of file to parse</param>
    /// <returns>File parser implementation</returns>
    public IFileParser GetParser(FileType fileType)
    {
        _logger.LogDebug("Getting parser for file type: {FileType}", fileType);

        // Get all registered parsers
        var parsers = _serviceProvider.GetServices<IFileParser>();

        // Find parser by type name convention
        var parserTypeName = $"{fileType}FileParser";
        var parser = parsers.FirstOrDefault(p => p.GetType().Name.Equals(parserTypeName, StringComparison.OrdinalIgnoreCase));

        if (parser == null)
        {
            _logger.LogError("No parser found for file type: {FileType}", fileType);
            throw new NotSupportedException($"No parser available for file type: {fileType}");
        }

        _logger.LogInformation("Selected parser: {ParserType} for {FileType}", parser.GetType().Name, fileType);
        return parser;
    }

    /// <summary>
    /// Detect file type from file extension
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>Detected file type</returns>
    public static FileType DetectFileType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".xlsx" => FileType.XLSX,
            ".csv" => FileType.CSV,
            ".txt" when Path.GetFileName(filePath).Contains("fixed", StringComparison.OrdinalIgnoreCase) => FileType.FixedLength,
            ".txt" => FileType.TXT,
            _ => throw new NotSupportedException($"File extension {extension} is not supported")
        };
    }
}
