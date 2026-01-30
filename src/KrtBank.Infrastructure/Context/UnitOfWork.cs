using KrtBank.Infrastructure.Context;

public class UnitOfWork : IUnitOfWork
{
    private readonly KrtBankDbContext _context;

    public UnitOfWork(KrtBankDbContext context) => _context = context;

    public Task BeginTransactionAsync(CancellationToken ct) => _context.Database.BeginTransactionAsync(ct);
    public Task CommitAsync(CancellationToken ct) => _context.Database.CurrentTransaction.CommitAsync(ct);
    public Task RollbackAsync(CancellationToken ct) => _context.Database.CurrentTransaction.RollbackAsync(ct);
    public Task<int> SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
    public bool HasChanges()
    {
        return _context.ChangeTracker.HasChanges();
    }

}