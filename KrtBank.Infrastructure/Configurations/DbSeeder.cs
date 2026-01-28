using KrtBank.Domain.Entities;
using KrtBank.Domain.Interfaces;
using KrtBank.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KrtBank.Infrastructure.Configurations
{
    public static class DbSeeder
    {
        public static async Task SeedUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KrtBankDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();

            const string adminEmail = "admin@admin.com";

            if (!(await context.Users.AnyAsync(u => u.Email == adminEmail)))
            {
                var user = new User(
                    name: "Administrador KrtBank",
                    email: adminEmail,
                    password: passwordService.HashPassword("Krt@2026") // Senha hasheada
                );

                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
        }
    }
}
