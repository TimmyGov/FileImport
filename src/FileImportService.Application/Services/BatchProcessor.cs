using FileImportService.Domain.Entities;
using FileImportService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace FileImportService.Application.Services;

/// <summary>
/// Service for handling batch operations
/// </summary>
public class BatchProcessor
{
    private readonly IStagingRepository _repository;
    private readonly ILogger<BatchProcessor> _logger;

    public BatchProcessor(
        IStagingRepository repository,
        ILogger<BatchProcessor> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Save records in batches
    /// </summary>
    /// <param name="records">Records to save</param>
    /// <param name="batchSize">Size of each batch</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task SaveInBatchesAsync(
        IEnumerable<StagingRecord> records,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var recordsList = records.ToList();
        var totalBatches = (int)Math.Ceiling((double)recordsList.Count / batchSize);

        _logger.LogInformation(
            "Saving {TotalRecords} records in {TotalBatches} batches of {BatchSize}",
            recordsList.Count,
            totalBatches,
            batchSize);

        for (int i = 0; i < recordsList.Count; i += batchSize)
        {
            var batch = recordsList.Skip(i).Take(batchSize).ToList();
            var batchNumber = (i / batchSize) + 1;

            _logger.LogDebug("Saving batch {BatchNumber}/{TotalBatches} with {RecordCount} records",
                batchNumber, totalBatches, batch.Count);

            await _repository.SaveAsync(batch, cancellationToken);

            _logger.LogInformation("Saved batch {BatchNumber}/{TotalBatches}", batchNumber, totalBatches);
        }

        _logger.LogInformation("Completed saving all {TotalRecords} records", recordsList.Count);
    }
}
