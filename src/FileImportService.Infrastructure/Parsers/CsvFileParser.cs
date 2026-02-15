using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using FileImportService.Application.Configuration;
using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileImportService.Infrastructure.Parsers;

/// <summary>
/// Parser for CSV files using CsvHelper
/// </summary>
public class CsvFileParser : IFileParser
{
    private readonly FileProcessingOptions _options;
    private readonly ILogger<CsvFileParser> _logger;

    public CsvFileParser(
        IOptions<FileProcessingOptions> options,
        ILogger<CsvFileParser> logger)
    {
        _options = options.Value;
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
                FileType = FileType.CSV
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

            var csvConfig = _options.FileTypes.GetValueOrDefault("CSV") ?? new FileTypeConfiguration();
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = csvConfig.HasHeader,
                Delimiter = csvConfig.Delimiter
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            await csv.ReadAsync();
            if (csvConfig.HasHeader)
            {
                csv.ReadHeader();
            }

            var headers = csv.HeaderRecord?.ToList() ?? new List<string>();
            var rowNumber = csvConfig.HasHeader ? 2 : 1;
            var parsedRows = new List<ParsedRow>();

            while (await csv.ReadAsync())
            {
                var row = new ParsedRow { RowNumber = rowNumber };

                if (headers.Any())
                {
                    foreach (var header in headers)
                    {
                        row.Values[header] = csv.GetField(header) ?? string.Empty;
                    }
                }
                else
                {
                    // No headers - use column indices
                    for (int i = 0; i < csv.Parser.Count; i++)
                    {
                        row.Values[$"Column{i + 1}"] = csv.GetField(i) ?? string.Empty;
                    }
                }

                parsedRows.Add(row);
                rowNumber++;
            }

            stopwatch.Stop();
            result.ParsedRows = parsedRows;
            result.Success = true;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogInformation(
                "Successfully parsed {RowCount} rows from CSV file {FileName}",
                parsedRows.Count,
                result.Metadata.FileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogError(ex, "Error parsing CSV file {FileName}", result.Metadata.FileName);
        }

        return result;
    }
}
