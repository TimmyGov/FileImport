# File Import Service

A production-ready file import service built with C# and .NET 9.0 that processes multiple file types (XLSX, CSV, TXT, Fixed-Length) dropped into a monitored directory. The solution follows clean architecture principles, SOLID design patterns, and modern development best practices.

## Architecture Overview

The solution is organized into five projects following clean architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Worker Service                             │
│         (Background Service, DI, Configuration)               │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                   Infrastructure Layer                        │
│     (Parsers, Repository, File System Services)              │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                   Application Layer                          │
│      (Business Logic, Orchestration, Services)               │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                     Domain Layer                             │
│          (Interfaces, Models, Entities, Enums)               │
└─────────────────────────────────────────────────────────────┘
```

### Project Structure

1. **FileImportService.Domain** - Core domain models, interfaces, and enums with no external dependencies
2. **FileImportService.Application** - Business logic, service orchestration, and configuration models
3. **FileImportService.Infrastructure** - File parsers, EF Core repository, and file system services
4. **FileImportService.Worker** - Background service host with FileSystemWatcher and DI configuration
5. **FileImportService.Tests** - Comprehensive unit and integration tests using xUnit, Moq, and FluentAssertions

## Features

### Supported File Types

- **CSV** - Comma-separated values with configurable delimiter and header options
- **XLSX** - Microsoft Excel files using ClosedXML
- **TXT** - Plain text files with line-by-line processing
- **Fixed-Length** - Fixed-width column format with configurable column definitions

### Key Capabilities

✅ **Automatic File Detection** - Monitors directory for new files using FileSystemWatcher  
✅ **Factory Pattern** - Automatically selects appropriate parser based on file type  
✅ **Data Validation** - Validates files and extracted data before processing  
✅ **Batch Processing** - Handles large files with configurable batch sizes  
✅ **Database Staging** - Stores parsed data in SQL Server staging table  
✅ **File Archiving** - Moves processed files to success/error folders with timestamps  
✅ **Structured Logging** - Comprehensive logging with Serilog and correlation IDs  
✅ **Error Handling** - Robust error handling with retry logic and graceful degradation  
✅ **Async/Await** - All I/O operations are fully asynchronous  
✅ **Dependency Injection** - Uses built-in .NET DI container throughout  

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (included with Visual Studio) or SQL Server instance
- Windows, Linux, or macOS

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/TimmyGov/FileImport.git
cd FileImport
```

### 2. Configure Connection String

Edit `src/FileImportService.Worker/appsettings.json` and update the connection string if needed:

```json
{
  "ConnectionStrings": {
    "StagingDatabase": "Server=(localdb)\\mssqllocaldb;Database=FileImportStaging;Trusted_Connection=True;"
  }
}
```

### 3. Configure Folders

Update the folder paths in `appsettings.json`:

```json
{
  "FileProcessing": {
    "WatchFolder": "C:\\FileImports\\Incoming",
    "ProcessedFolder": "C:\\FileImports\\Processed",
    "ErrorFolder": "C:\\FileImports\\Error"
  }
}
```

### 4. Create Directories

```bash
mkdir C:\FileImports\Incoming
mkdir C:\FileImports\Processed
mkdir C:\FileImports\Error
```

### 5. Restore Packages

```bash
dotnet restore
```

### 6. Run Database Migrations

```bash
cd src/FileImportService.Infrastructure
dotnet ef database update --startup-project ../FileImportService.Worker
```

Or the database will be automatically created when the worker starts.

### 7. Build the Solution

```bash
dotnet build
```

### 8. Run Tests

```bash
dotnet test
```

### 9. Run the Worker Service

```bash
cd src/FileImportService.Worker
dotnet run
```

## Configuration Guide

### File Processing Options

```json
{
  "FileProcessing": {
    "WatchFolder": "C:\\FileImports\\Incoming",          // Directory to monitor
    "ProcessedFolder": "C:\\FileImports\\Processed",     // Success folder
    "ErrorFolder": "C:\\FileImports\\Error",             // Error folder
    "BatchSize": 1000,                                    // Records per batch
    "MaxRetries": 3,                                      // Retry attempts
    "PollingIntervalSeconds": 5,                          // Polling frequency
    "FileTypes": {
      "CSV": {
        "HasHeader": true,                                // CSV has headers
        "Delimiter": ","                                  // Delimiter character
      },
      "FixedLength": {
        "ColumnDefinitions": [
          { "Name": "Field1", "Start": 0, "Length": 10 },
          { "Name": "Field2", "Start": 10, "Length": 20 },
          { "Name": "Field3", "Start": 30, "Length": 15 }
        ]
      }
    }
  }
}
```

### Logging Configuration

Serilog is configured to write to both console and rolling file:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/fileimport-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

## Usage Examples

### Processing a CSV File

1. Create a CSV file with headers:
```csv
Id,Name,Email,Amount,Date
1,John Doe,john@example.com,1000.50,2026-01-15
2,Jane Smith,jane@example.com,2500.75,2026-01-16
```

2. Drop the file into the `WatchFolder`
3. The service will automatically:
   - Detect the file
   - Parse the CSV data
   - Validate the data
   - Save to staging database
   - Move to processed folder

### Viewing Logs

```bash
tail -f logs/fileimport-20260215.log
```

### Querying Staging Table

```sql
-- View all records for a batch
SELECT * FROM FileImportStaging 
WHERE BatchId = 'your-batch-guid'
ORDER BY RowNumber;

-- View processing statistics
SELECT ProcessedStatus, COUNT(*) as Count
FROM FileImportStaging
GROUP BY ProcessedStatus;
```

## Database Schema

```sql
CREATE TABLE FileImportStaging (
    Id BIGINT PRIMARY KEY IDENTITY,
    BatchId UNIQUEIDENTIFIER NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FileType NVARCHAR(10) NOT NULL,
    RowNumber INT NOT NULL,
    RawData NVARCHAR(MAX),              -- JSON
    ProcessedStatus NVARCHAR(20) NOT NULL,
    ValidationErrors NVARCHAR(MAX),
    ImportedAt DATETIME2 NOT NULL,
    ProcessedAt DATETIME2,
    INDEX IX_BatchId (BatchId),
    INDEX IX_ProcessedStatus (ProcessedStatus)
)
```

## Extending the Service

### Adding a New File Type

1. **Create a new parser** in `Infrastructure/Parsers`:

```csharp
public class JsonFileParser : IFileParser
{
    public async Task<FileImportResult> ParseAsync(string filePath, 
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

2. **Register the parser** in `Program.cs`:

```csharp
builder.Services.AddScoped<IFileParser, JsonFileParser>();
```

3. **Update file type detection** in `FileParserFactory`:

```csharp
public static FileType DetectFileType(string filePath)
{
    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    return extension switch
    {
        ".json" => FileType.JSON,
        // ... other types
    };
}
```

### Adding Custom Validation

Implement `IDataValidator` with custom validation logic:

```csharp
public class CustomDataValidator : IDataValidator
{
    public Task<ValidationResult> ValidateDataAsync(
        IEnumerable<ParsedRow> rows, 
        CancellationToken cancellationToken = default)
    {
        // Custom validation logic
    }
}
```

## Testing

### Running All Tests

```bash
dotnet test
```

### Running Specific Test Category

```bash
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
```

### Test Coverage

The solution includes:
- Unit tests for all parsers (CSV, XLSX, TXT, Fixed-Length)
- Unit tests for services (Factory, Validation, Batch Processing)
- Integration tests with sample files
- Mock repository tests using Moq

Sample test files are located in `tests/FileImportService.Tests/SampleFiles/`.

## Troubleshooting

### Issue: Database Connection Failed

**Solution**: Ensure SQL Server LocalDB is installed and running:
```bash
sqllocaldb info
sqllocaldb start mssqllocaldb
```

### Issue: Files Not Being Processed

**Solution**: 
1. Check that folders exist and worker has permissions
2. Verify FileSystemWatcher is monitoring correct folder
3. Check logs for errors: `logs/fileimport-*.log`

### Issue: Parser Not Found Error

**Solution**: Ensure the parser is registered in DI container in `Program.cs`.

## Performance Considerations

- **Batch Size**: Adjust `BatchSize` based on available memory (default: 1000)
- **Polling Interval**: Reduce for faster processing, increase to reduce CPU usage
- **Database**: Use appropriate SQL Server edition for production workloads
- **Concurrency**: Currently processes one file at a time; extend for parallel processing if needed

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues and questions:
- Open an issue on GitHub
- Check the logs in `logs/` directory
- Review the test cases for usage examples
