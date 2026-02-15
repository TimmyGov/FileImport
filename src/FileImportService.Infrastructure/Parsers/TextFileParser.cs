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

        var result = new FileImportResult
        {
            Metadata = new FileMetadata
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileType = FileType.TXT
            },
            Success = false
        };

        try
        {
            if (!File.Exists(filePath))
            {
                result.ErrorMessage = $"File not found: {filePath}";
                stopwatch.Stop();
                result.ParseDuration = stopwatch.Elapsed;
                return result;
            }

            var fileInfo = new FileInfo(filePath);
            result.Metadata.FileSize = fileInfo.Length;
            result.Metadata.CreatedAt = fileInfo.CreationTimeUtc;
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
                result.Metadata.FileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogError(ex, "Error parsing text file {FileName}", result.Metadata.FileName);
        }

        return result;
    }
}
