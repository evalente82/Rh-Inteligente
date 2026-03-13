using Domain.Entities;
using Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// DbContext principal do Vcorp Folha IA.
/// Implementa Global Query Filter por EmpresaId em todas as entidades transacionais (Regra 5).
/// O EmpresaId do tenant ativo é resolvido via ITenantProvider, injetado pelo middleware de tenant.
/// </summary>
public sealed class RhInteligenteDbContext : DbContext
{
    private readonly Guid _empresaId;

    public RhInteligenteDbContext(DbContextOptions<RhInteligenteDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _empresaId = tenantProvider.EmpresaId;
    }

    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<RegistroPonto> RegistrosPonto => Set<RegistroPonto>();
    public DbSet<AlertaAnomalia> AlertasAnomalia => Set<AlertaAnomalia>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações IEntityTypeConfiguration<T> deste assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RhInteligenteDbContext).Assembly);

        // ─── Global Query Filters (Regra 5 — Multi-tenant inegociável) ──────────
        modelBuilder.Entity<Funcionario>()
            .HasQueryFilter(f => f.EmpresaId == _empresaId);

        modelBuilder.Entity<RegistroPonto>()
            .HasQueryFilter(r => r.EmpresaId == _empresaId);

        modelBuilder.Entity<AlertaAnomalia>()
            .HasQueryFilter(a => a.EmpresaId == _empresaId);

        base.OnModelCreating(modelBuilder);
    }
}
