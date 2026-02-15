using FileImportService.Application.Configuration;
using FileImportService.Application.Services;
using FileImportService.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace FileImportService.Worker;

/// <summary>
/// Background worker service for file import processing
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IFileWatcher _fileWatcher;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly FileProcessingOptions _options;

    public Worker(
        ILogger<Worker> logger,
        IFileWatcher fileWatcher,
        IServiceScopeFactory scopeFactory,
        IOptions<FileProcessingOptions> options)
    {
        _logger = logger;
        _fileWatcher = fileWatcher;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File Import Worker starting at: {Time}", DateTimeOffset.Now);
        _logger.LogInformation("Watching folder: {WatchFolder}", _options.WatchFolder);

        try
        {
            // Start watching for file changes
            _fileWatcher.StartWatching(_options.WatchFolder, async (filePath) =>
            {
                try
                {
                    _logger.LogInformation("Processing file: {FilePath}", filePath);
                    
                    // Create a scope for scoped services
                    using var scope = _scopeFactory.CreateScope();
                    var fileProcessingService = scope.ServiceProvider.GetRequiredService<FileProcessingService>();
                    
                    await fileProcessingService.ProcessFileAsync(filePath, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing file {FilePath}", filePath);
                }
            });

            _logger.LogInformation("File Import Worker started successfully");

            // Keep the service running
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker service is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error in worker service");
            throw;
        }
        finally
        {
            _fileWatcher.StopWatching();
            _logger.LogInformation("File Import Worker stopped at: {Time}", DateTimeOffset.Now);
        }
    }
}
