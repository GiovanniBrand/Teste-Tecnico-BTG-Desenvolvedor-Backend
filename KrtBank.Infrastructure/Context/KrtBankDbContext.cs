using KrtBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KrtBank.Infrastructure.Context;

public class KrtBankDbContext : DbContext
{
    public KrtBankDbContext(DbContextOptions<KrtBankDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KrtBankDbContext).Assembly);
    }
}
