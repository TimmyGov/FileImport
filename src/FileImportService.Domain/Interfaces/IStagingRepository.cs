using FileImportService.Domain.Entities;

namespace FileImportService.Domain.Interfaces;

/// <summary>
/// Repository interface for staging records
/// </summary>
public interface IStagingRepository
{
    /// <summary>
    /// Save records to staging table
    /// </summary>
    /// <param name="records">Records to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task SaveAsync(IEnumerable<StagingRecord> records, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get records by batch ID
    /// </summary>
    /// <param name="batchId">Batch identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of staging records</returns>
    Task<List<StagingRecord>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default);
}
