using FileImportService.Domain.Enums;

namespace FileImportService.Domain.Models;

/// <summary>
/// Metadata about a file being processed
/// </summary>
public class FileMetadata
{
    /// <summary>
    /// Full path to the file
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// File name without path
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    /// Type of the file
    /// </summary>
    public required FileType FileType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// File creation time
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Unique batch identifier for this file import
    /// </summary>
    public Guid BatchId { get; set; } = Guid.NewGuid();
}
