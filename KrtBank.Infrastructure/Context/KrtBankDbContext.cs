using Microsoft.EntityFrameworkCore;

namespace KrtBank.Infrastructure.Context;

public class KrtBankDbContext : DbContext
{
    public KrtBankDbContext(DbContextOptions<KrtBankDbContext> options) : base(options)
    {
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KrtBankDbContext).Assembly);
    }
}
