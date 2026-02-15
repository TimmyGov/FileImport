using System.Diagnostics;
using ClosedXML.Excel;
using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FileImportService.Infrastructure.Parsers;

/// <summary>
/// Parser for Excel (XLSX) files using ClosedXML
/// </summary>
public class XlsxFileParser : IFileParser
{
    private readonly ILogger<XlsxFileParser> _logger;

    public XlsxFileParser(ILogger<XlsxFileParser> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<FileImportResult> ParseAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ParseWorkbook(filePath), cancellationToken);
    }

    private FileImportResult ParseWorkbook(string filePath)
    {
        var stopwatch = Stopwatch.StartNew();

        var result = new FileImportResult
        {
            Metadata = new FileMetadata
            {
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                FileType = FileType.XLSX
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
            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheets.First();

            var firstRow = worksheet.FirstRowUsed();
            if (firstRow == null)
            {
                result.ErrorMessage = "Worksheet is empty";
                stopwatch.Stop();
                result.ParseDuration = stopwatch.Elapsed;
                return result;
            }

            // Get headers from first row
            var headers = new List<string>();
            var headerRow = firstRow;
            foreach (var cell in headerRow.CellsUsed())
            {
                headers.Add(cell.GetString());
            }

            var parsedRows = new List<ParsedRow>();
            var rowNumber = 2; // Start from row 2 (after header)

            foreach (var row in worksheet.RowsUsed().Skip(1)) // Skip header row
            {
                var parsedRow = new ParsedRow { RowNumber = rowNumber };

                var cellIndex = 0;
                foreach (var cell in row.CellsUsed())
                {
                    var columnIndex = cell.Address.ColumnNumber - 1;
                    var header = columnIndex < headers.Count ? headers[columnIndex] : $"Column{columnIndex + 1}";
                    parsedRow.Values[header] = cell.GetString();
                    cellIndex++;
                }

                parsedRows.Add(parsedRow);
                rowNumber++;
            }

            stopwatch.Stop();
            result.ParsedRows = parsedRows;
            result.Success = true;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogInformation(
                "Successfully parsed {RowCount} rows from XLSX file {FileName}",
                parsedRows.Count,
                result.Metadata.FileName);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ParseDuration = stopwatch.Elapsed;

            _logger.LogError(ex, "Error parsing XLSX file {FileName}", result.Metadata.FileName);
        }

        return result;
    }
}
