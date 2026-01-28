using KrtBank.Domain.Interfaces;
using KrtBank.Domain.Repositories;
using KrtBank.Infrastructure.Configurations;
using KrtBank.Infrastructure.Context;
using KrtBank.Infrastructure.Repositories;
using KrtBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace KrtBank.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "KrtBank";
        });

        services.AddDbContext<KrtBankDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(KrtBankDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        AddServices(services);

        return services;
    }

    private static void AddServices(IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null && t.Namespace.Contains("Services"));

        foreach (var type in serviceTypes)
        {
            var interfaceType = type.GetInterface($"I{type.Name}");

            if (interfaceType != null)
            {
                services.AddScoped(interfaceType, type);
            }
        }
    }
}