using CarDealership.Data.Seeds;

namespace CarDealership.Services;

public class StartupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupService> _logger;

    public StartupService(
        IServiceProvider serviceProvider,
        ILogger<StartupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Startup service started");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dataSeeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();

            // Seed all data
            await dataSeeder.SeedAllAsync();

            _logger.LogInformation("Startup service completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during startup service execution");
        }
    }
}
