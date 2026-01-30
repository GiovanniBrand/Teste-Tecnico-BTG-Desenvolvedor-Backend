using KrtBank.Domain.Entities;
using KrtBank.Domain.Enums;
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

        public static async Task SeedAccountsAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<KrtBankDbContext>();

            // 🛡️ TRAVA DE SEGURANÇA: Só continua se a tabela estiver 100% vazia
            if (await context.Accounts.AnyAsync())
            {
                return; // Sai do método sem fazer nada
            }

            var accounts = new List<Account>
            {
                new Account("João Silva", "12345678901", AccountStatus.Active),
                new Account("Maria Oliveira", "23456789012", AccountStatus.Active),
                new Account("Pedro Santos", "34567890123", AccountStatus.Inactive),
                new Account("Ana Costa", "45678901234", AccountStatus.Active),
                new Account("Carlos Pereira", "56789012345", AccountStatus.Active),
                new Account("Julia Ferreira", "67890123456", AccountStatus.Inactive),
                new Account("Ricardo Almeida", "78901234567", AccountStatus.Active),
                new Account("Beatriz Souza", "89012345678", AccountStatus.Active)
            };

            await context.Accounts.AddRangeAsync(accounts);
            await context.SaveChangesAsync();
        }
    }
}

