using FileImportService.Domain.Enums;

namespace FileImportService.Domain.Entities;

/// <summary>
/// Staging record entity for database
/// </summary>
public class StagingRecord
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Batch identifier for grouping records from the same file
    /// </summary>
    public Guid BatchId { get; set; }

    /// <summary>
    /// Source file name
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// File type
    /// </summary>
    public required string FileType { get; set; }

    /// <summary>
    /// Row number in source file
    /// </summary>
    public int RowNumber { get; set; }

    /// <summary>
    /// Raw data as JSON
    /// </summary>
    public string? RawData { get; set; }

    /// <summary>
    /// Processing status
    /// </summary>
    public required string ProcessedStatus { get; set; }

    /// <summary>
    /// Validation errors if any
    /// </summary>
    public string? ValidationErrors { get; set; }

    /// <summary>
    /// When the record was imported
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the record was processed
    /// </summary>
    public DateTime? ProcessedAt { get; set; }
}
