namespace FileImportService.Application.Configuration;

/// <summary>
/// File processing configuration options
/// </summary>
public class FileProcessingOptions
{
    /// <summary>
    /// Folder to watch for incoming files
    /// </summary>
    public required string WatchFolder { get; set; }

    /// <summary>
    /// Folder for successfully processed files
    /// </summary>
    public required string ProcessedFolder { get; set; }

    /// <summary>
    /// Folder for files that failed processing
    /// </summary>
    public required string ErrorFolder { get; set; }

    /// <summary>
    /// Maximum number of records to process in a single batch
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Maximum number of retry attempts for failed files
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Polling interval in seconds
    /// </summary>
    public int PollingIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// File type specific configurations
    /// </summary>
    public Dictionary<string, FileTypeConfiguration> FileTypes { get; set; } = new();
}
