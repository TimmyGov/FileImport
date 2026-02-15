namespace FileImportService.Domain.Enums;

/// <summary>
/// Supported file types for import
/// </summary>
public enum FileType
{
    /// <summary>
    /// Excel files (.xlsx)
    /// </summary>
    XLSX,

    /// <summary>
    /// Comma-separated values files (.csv)
    /// </summary>
    CSV,

    /// <summary>
    /// Plain text files (.txt)
    /// </summary>
    TXT,

    /// <summary>
    /// Fixed-length format files
    /// </summary>
    FixedLength
}
