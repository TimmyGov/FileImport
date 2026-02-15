using FileImportService.Domain.Entities;
using FileImportService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FileImportService.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for staging records using Entity Framework Core
/// </summary>
public class StagingRepository : IStagingRepository
{
    private readonly StagingDbContext _context;
    private readonly ILogger<StagingRepository> _logger;

    public StagingRepository(
        StagingDbContext context,
        ILogger<StagingRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<StagingRecord> records, CancellationToken cancellationToken = default)
    {
        var recordsList = records.ToList();

        _logger.LogDebug("Saving {RecordCount} records to staging table", recordsList.Count);

        await _context.FileImportStaging.AddRangeAsync(recordsList, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Successfully saved {RecordCount} records", recordsList.Count);
    }

    /// <inheritdoc/>
    public async Task<List<StagingRecord>> GetByBatchIdAsync(Guid batchId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving records for BatchId: {BatchId}", batchId);

        return await _context.FileImportStaging
            .Where(r => r.BatchId == batchId)
            .OrderBy(r => r.RowNumber)
            .ToListAsync(cancellationToken);
    }
}
