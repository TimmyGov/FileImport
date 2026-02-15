using System.Diagnostics;
using System.Text.Json;
using FileImportService.Application.Configuration;
using FileImportService.Domain.Entities;
using FileImportService.Domain.Enums;
using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileImportService.Application.Services;

/// <summary>
/// Main file processing orchestration service
/// </summary>
public class FileProcessingService
{
    private readonly FileParserFactory _parserFactory;
    private readonly IDataValidator _dataValidator;
    private readonly BatchProcessor _batchProcessor;
    private readonly IFileArchiver _fileArchiver;
    private readonly FileProcessingOptions _options;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        FileParserFactory parserFactory,
        IDataValidator dataValidator,
        BatchProcessor batchProcessor,
        IFileArchiver fileArchiver,
        IOptions<FileProcessingOptions> options,
        ILogger<FileProcessingService> logger)
    {
        _parserFactory = parserFactory;
        _dataValidator = dataValidator;
        _batchProcessor = batchProcessor;
        _fileArchiver = fileArchiver;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Process a file through the complete workflow
    /// </summary>
    /// <param name="filePath">Path to file to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task ProcessFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var batchId = Guid.NewGuid();
        var fileName = Path.GetFileName(filePath);

        _logger.LogInformation("Processing file {FileName} with BatchId {BatchId}", fileName, batchId);

        try
        {
            // Detect file type
            var fileType = FileParserFactory.DetectFileType(filePath);
            _logger.LogInformation("Detected file type: {FileType} for {FileName}", fileType, fileName);

            // Get appropriate parser
            var parser = _parserFactory.GetParser(fileType);

            // Parse file
            _logger.LogInformation("Parsing file {FileName}", fileName);
            var parseResult = await parser.ParseAsync(filePath, cancellationToken);

            if (!parseResult.Success)
            {
                _logger.LogError("Failed to parse file {FileName}: {Error}", fileName, parseResult.ErrorMessage);
                await _fileArchiver.ArchiveFileAsync(filePath, _options.ErrorFolder, false, cancellationToken);
                return;
            }

            _logger.LogInformation(
                "Parsed {RowCount} rows from {FileName} in {Duration}ms",
                parseResult.ParsedRows.Count,
                fileName,
                parseResult.ParseDuration.TotalMilliseconds);

            // Validate data
            var validationResult = await _dataValidator.ValidateDataAsync(parseResult.ParsedRows, cancellationToken);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Validation failed for {RowCount} rows in batch {BatchId}",
                    parseResult.ParsedRows.Count,
                    batchId);
            }

            // Convert to staging records
            var stagingRecords = ConvertToStagingRecords(
                parseResult.ParsedRows,
                batchId,
                fileName,
                fileType,
                validationResult);

            // Save to database in batches
            _logger.LogInformation("Saving {RecordCount} records to staging database", stagingRecords.Count);
            await _batchProcessor.SaveInBatchesAsync(stagingRecords, _options.BatchSize, cancellationToken);

            // Archive file
            await _fileArchiver.ArchiveFileAsync(filePath, _options.ProcessedFolder, true, cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Successfully processed file {FileName} with BatchId {BatchId} in {Duration}ms",
                fileName,
                batchId,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "Error processing file {FileName} with BatchId {BatchId}: {Error}",
                fileName,
                batchId,
                ex.Message);

            try
            {
                await _fileArchiver.ArchiveFileAsync(filePath, _options.ErrorFolder, false, cancellationToken);
            }
            catch (Exception archiveEx)
            {
                _logger.LogError(archiveEx, "Failed to archive error file {FileName}", fileName);
            }

            throw;
        }
    }

    private List<StagingRecord> ConvertToStagingRecords(
        List<ParsedRow> parsedRows,
        Guid batchId,
        string fileName,
        FileType fileType,
        ValidationResult validationResult)
    {
        return parsedRows.Select(row => new StagingRecord
        {
            BatchId = batchId,
            FileName = fileName,
            FileType = fileType.ToString(),
            RowNumber = row.RowNumber,
            RawData = JsonSerializer.Serialize(row.Values),
            ProcessedStatus = validationResult.IsValid ? ProcessStatus.Valid.ToString() : ProcessStatus.Invalid.ToString(),
            ValidationErrors = validationResult.IsValid ? null : string.Join("; ", validationResult.Errors),
            ImportedAt = DateTime.UtcNow
        }).ToList();
    }
}
