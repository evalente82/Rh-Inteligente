using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// Factory de DbContext para uso em contextos de sistema (DataSeeder, jobs, migrations).
/// Cria instâncias de RhInteligenteDbContext com SystemTenantProvider (EmpresaId = Guid.Empty),
/// desabilitando os Global Query Filters de tenant para acesso cross-tenant.
/// </summary>
internal sealed class SeedDbContextFactory : IDbContextFactory<RhInteligenteDbContext>
{
    private readonly DbContextOptions<RhInteligenteDbContext> _options;
    private readonly SystemTenantProvider _systemTenant = new();

    public SeedDbContextFactory(DbContextOptions<RhInteligenteDbContext> options)
    {
        _options = options;
    }

    public RhInteligenteDbContext CreateDbContext()
        => new(_options, _systemTenant);
}
