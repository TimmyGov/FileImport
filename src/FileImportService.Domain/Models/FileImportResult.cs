namespace FileImportService.Domain.Models;

/// <summary>
/// Result of file import operation
/// </summary>
public class FileImportResult
{
    /// <summary>
    /// File metadata
    /// </summary>
    public required FileMetadata Metadata { get; set; }

    /// <summary>
    /// List of parsed rows
    /// </summary>
    public List<ParsedRow> ParsedRows { get; set; } = new();

    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if import failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time taken to parse the file
    /// </summary>
    public TimeSpan ParseDuration { get; set; }
}
