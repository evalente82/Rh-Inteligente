using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 do repositório de <see cref="Empresa"/>.
/// Empresa é um agregado global (sem EmpresaId próprio) — sem Global Query Filter.
/// </summary>
internal sealed class EmpresaRepository : IEmpresaRepository
{
    private readonly RhInteligenteDbContext _context;

    public EmpresaRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<Empresa?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Empresas.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<Empresa?> ObterPorCnpjAsync(string cnpj, CancellationToken ct = default)
        => await _context.Empresas.FirstOrDefaultAsync(e => e.Cnpj == cnpj, ct);

    public async Task AdicionarAsync(Empresa empresa, CancellationToken ct = default)
        => await _context.Empresas.AddAsync(empresa, ct);
}
