using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RavApi.Infrastructure;

namespace RavApi.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            LocalEnvFileLoader.TryLoad(Directory.GetCurrentDirectory());

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .AddEnvironmentVariables();

            if (Directory.Exists("/run/secrets"))
            {
                configurationBuilder.AddKeyPerFile("/run/secrets", optional: true);
            }

            var configuration = configurationBuilder.Build();

            var connectionString = DatabaseConnectionStringFactory.Resolve(configuration);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
