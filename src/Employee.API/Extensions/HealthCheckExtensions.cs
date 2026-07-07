using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Employee.API.Extensions
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection AddHealthCheckServices(
            this IServiceCollection services,
            IConfiguration configuration = null)
        {
            var healthCheckBuilder = services
                .AddHealthChecks()
                .AddCheck(
                    "API Health",
                    () => HealthCheckResult.Healthy("API is running"),
                    tags: new[] { "api" });

            if (configuration != null && !string.IsNullOrEmpty(configuration.GetConnectionString("DefaultConnection")))
            {
                healthCheckBuilder.AddSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    name: "Database",
                    tags: new[] { "database" });
            }

            return services;
        }
    }
}
