using FileImportService.Application.Configuration;
using FileImportService.Application.Services;
using FileImportService.Domain.Interfaces;
using FileImportService.Infrastructure.FileSystem;
using FileImportService.Infrastructure.Parsers;
using FileImportService.Infrastructure.Persistence;
using FileImportService.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting File Import Service");

    var builder = Host.CreateApplicationBuilder(args);

    // Add Serilog
    builder.Services.AddSerilog();

    // Configure options
    builder.Services.Configure<FileProcessingOptions>(
        builder.Configuration.GetSection("FileProcessing"));

    // Add DbContext
    builder.Services.AddDbContext<StagingDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("StagingDatabase")));

    // Register Domain interfaces
    builder.Services.AddScoped<IStagingRepository, StagingRepository>();
    builder.Services.AddScoped<IDataValidator, DataValidationService>();
    builder.Services.AddSingleton<IFileWatcher, FileSystemWatcherService>();
    builder.Services.AddScoped<IFileArchiver, FileArchiver>();

    // Register all parsers
    builder.Services.AddScoped<IFileParser, CsvFileParser>();
    builder.Services.AddScoped<IFileParser, XlsxFileParser>();
    builder.Services.AddScoped<IFileParser, TextFileParser>();
    builder.Services.AddScoped<IFileParser, FixedLengthFileParser>();

    // Register Application services
    builder.Services.AddScoped<FileParserFactory>();
    builder.Services.AddScoped<BatchProcessor>();
    builder.Services.AddScoped<FileProcessingService>();

    // Register Worker
    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();

    // Ensure database is created
    using (var scope = host.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<StagingDbContext>();
        dbContext.Database.EnsureCreated();
        Log.Information("Database initialized");
    }

    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
