namespace FileImportService.Domain.Models;

/// <summary>
/// Result of validation operation
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether validation was successful
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation error messages
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Creates a successful validation result
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with errors
    /// </summary>
    public static ValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };

    /// <summary>
    /// Creates a failed validation result with error list
    /// </summary>
    public static ValidationResult Failure(List<string> errors) =>
        new() { IsValid = false, Errors = errors };
}
