using System.Diagnostics;
using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FileImportService.Infrastructure.Parsers;

/// <summary>
/// Parser for plain text files
/// </summary>
public class TextFileParser : IFileParser
{
    private readonly ILogger<TextFileParser> _logger;

    public TextFileParser(ILogger<TextFileParser> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FileImportResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var fileInfo = new FileInfo(filePath);

        var metadata = new FileMetadata
        {
            FilePath = filePath,
            FileName = fileInfo.Name,
            FileType = FileType.TXT,
            FileSize = fileInfo.Length,
            CreatedAt = fileInfo.CreationTimeUtc
        };

        var result = new FileImportResult
        {
            Metadata = metadata,
            Success = false
        };

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
            var parsedRows = new List<ParsedRow>();
            var rowNumber = 1;

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var row = new ParsedRow { RowNumber = rowNumber };
                    row.Values["Text"] = line;
                    parsedRows.Add(row);
                }
                rowNumber++;
            }

            stopwatch.Stop();
            result.ParsedRows = parsedRows;
            result.Success = true;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogInformation(
                "Successfully parsed {RowCount} rows from text file {FileName}",
                parsedRows.Count,
                metadata.FileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogError(ex, "Error parsing text file {FileName}", metadata.FileName);
        }

        return result;
    }
}
