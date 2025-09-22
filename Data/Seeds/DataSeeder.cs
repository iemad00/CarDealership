namespace CarDealership.Data.Seeds;

public class DataSeeder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IServiceProvider serviceProvider, ILogger<DataSeeder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedAllAsync()
    {
        _logger.LogInformation("Starting data seeding process");

        var seedServices = new List<ISeedService>
        {
            _serviceProvider.GetRequiredService<SuperAdminSeedService>()
        };

        foreach (var seedService in seedServices)
        {
            try
            {
                _logger.LogInformation("Running {ServiceName}...", seedService.GetType().Name);
                await seedService.SeedAsync();
                _logger.LogInformation("{ServiceName} completed successfully", seedService.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {ServiceName}", seedService.GetType().Name);
                throw;
            }
        }

        _logger.LogInformation("Data seeding process completed");
    }
}
