using System.Diagnostics;
using FileImportService.Application.Configuration;
using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileImportService.Infrastructure.Parsers;

/// <summary>
/// Parser for fixed-length format files
/// </summary>
public class FixedLengthFileParser : IFileParser
{
    private readonly FileProcessingOptions _options;
    private readonly ILogger<FixedLengthFileParser> _logger;

    public FixedLengthFileParser(
        IOptions<FileProcessingOptions> options,
        ILogger<FixedLengthFileParser> logger)
    {
        _options = options.Value;
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
            FileType = FileType.FixedLength,
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
            var config = _options.FileTypes.GetValueOrDefault("FixedLength");
            if (config == null || !config.ColumnDefinitions.Any())
            {
                result.ErrorMessage = "No column definitions found for FixedLength file type";
                stopwatch.Stop();
                result.ParseDuration = stopwatch.Elapsed;
                return result;
            }

            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
            var parsedRows = new List<ParsedRow>();
            var rowNumber = 1;

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    var row = new ParsedRow { RowNumber = rowNumber };

                    foreach (var columnDef in config.ColumnDefinitions)
                    {
                        var value = ExtractField(line, columnDef.Start, columnDef.Length);
                        row.Values[columnDef.Name] = value;
                    }

                    parsedRows.Add(row);
                }
                rowNumber++;
            }

            stopwatch.Stop();
            result.ParsedRows = parsedRows;
            result.Success = true;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogInformation(
                "Successfully parsed {RowCount} rows from fixed-length file {FileName}",
                parsedRows.Count,
                metadata.FileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogError(ex, "Error parsing fixed-length file {FileName}", metadata.FileName);
        }

        return result;
    }

    private static string ExtractField(string line, int start, int length)
    {
        if (start >= line.Length)
            return string.Empty;

        var actualLength = Math.Min(length, line.Length - start);
        return line.Substring(start, actualLength).Trim();
    }
}
