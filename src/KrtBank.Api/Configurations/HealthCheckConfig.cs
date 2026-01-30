using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace KrtBank.Api.Configurations
{
    public static class HealthCheckConfig
    {
        public static void AddHealthCheckConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: configuration.GetConnectionString("DefaultConnection")!,
                    name: "SQL Server",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: ["db", "sql", "ready"])
                .AddRedis(
                    redisConnectionString: configuration.GetConnectionString("Redis")!,
                    name: "Redis Cache",
                    failureStatus: HealthStatus.Degraded,
                    tags: ["cache", "redis"]);
        }
    }
}
