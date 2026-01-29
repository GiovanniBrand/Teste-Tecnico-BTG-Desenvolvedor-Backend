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

        services.AddScoped<IRedisCacheService, RedisCacheService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}