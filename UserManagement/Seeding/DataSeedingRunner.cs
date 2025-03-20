// DataSeedingRunner.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace UserManagement.Seeding
{
    public class DataSeedingRunner : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataSeedingRunner> _logger;

        public DataSeedingRunner(
            IServiceProvider serviceProvider,
            ILogger<DataSeedingRunner> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting data seeding...");

            using var scope = _serviceProvider.CreateScope();
            var seeders = scope.ServiceProvider.GetServices<IDataSeeder>();
            seeders = seeders.OrderBy(e => e.OrderOfExecution);

            foreach (var seeder in seeders)
            {
                try
                {
                    _logger.LogInformation("Starting Seeding {seed-name}", seeder.GetType().Name);
                    await seeder.SeedAsync();
                    _logger.LogInformation("Finished Seeding {seed-name} succssfully", seeder.GetType().Name);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to seed data for {Seeder}", seeder.GetType().Name);
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}