using FileImportService.Domain.Interfaces;
using FileImportService.Domain.Models;
using Microsoft.Extensions.Logging;

namespace FileImportService.Application.Services;

/// <summary>
/// Service for validating extracted data
/// </summary>
public class DataValidationService : IDataValidator
{
    private readonly ILogger<DataValidationService> _logger;

    public DataValidationService(ILogger<DataValidationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<ValidationResult> ValidateDataAsync(
        IEnumerable<ParsedRow> rows,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var rowsList = rows.ToList();

        if (!rowsList.Any())
        {
            errors.Add("No data rows found");
            return Task.FromResult(ValidationResult.Failure(errors));
        }

        // Basic validation - check for empty rows
        foreach (var row in rowsList)
        {
            if (!row.Values.Any())
            {
                errors.Add($"Row {row.RowNumber} has no data");
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("Data validation failed with {ErrorCount} errors", errors.Count);
            return Task.FromResult(ValidationResult.Failure(errors));
        }

        _logger.LogInformation("Data validation successful for {RowCount} rows", rowsList.Count);
        return Task.FromResult(ValidationResult.Success());
    }
}
