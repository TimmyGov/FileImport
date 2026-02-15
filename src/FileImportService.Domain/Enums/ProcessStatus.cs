namespace FileImportService.Domain.Enums;

/// <summary>
/// Processing status for staged records
/// </summary>
public enum ProcessStatus
{
    /// <summary>
    /// Record is pending processing
    /// </summary>
    Pending,

    /// <summary>
    /// Record has been validated successfully
    /// </summary>
    Valid,

    /// <summary>
    /// Record failed validation
    /// </summary>
    Invalid,

    /// <summary>
    /// Record has been processed
    /// </summary>
    Processed,

    /// <summary>
    /// Record processing failed
    /// </summary>
    Failed
}
