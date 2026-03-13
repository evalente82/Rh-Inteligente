using Application.Interfaces;

namespace Infrastructure.Persistence;

/// <summary>
/// Implementação EF Core 8 do padrão Unit of Work.
/// Delega o commit para o DbContext.SaveChangesAsync, garantindo que todas
/// as operações de um Use Case sejam persistidas atomicamente.
/// </summary>
internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly RhInteligenteDbContext _context;

    public EfUnitOfWork(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
