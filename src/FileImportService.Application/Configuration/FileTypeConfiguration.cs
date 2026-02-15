namespace FileImportService.Application.Configuration;

/// <summary>
/// Configuration for specific file type
/// </summary>
public class FileTypeConfiguration
{
    /// <summary>
    /// Whether file has header row (for CSV)
    /// </summary>
    public bool HasHeader { get; set; } = true;

    /// <summary>
    /// Delimiter character (for CSV)
    /// </summary>
    public string Delimiter { get; set; } = ",";

    /// <summary>
    /// Column definitions for fixed-length files
    /// </summary>
    public List<FixedLengthColumnDefinition> ColumnDefinitions { get; set; } = new();
}
