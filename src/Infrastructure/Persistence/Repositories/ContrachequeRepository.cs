using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 de <see cref="IContrachequeRepository"/>.
/// O Global Query Filter por EmpresaId já é aplicado pelo DbContext (Regra 5).
/// </summary>
public sealed class ContrachequeRepository : IContrachequeRepository
{
    private readonly RhInteligenteDbContext _context;

    public ContrachequeRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<Contracheque?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Contracheques
            .Include("_itens")
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<Contracheque>> ListarPorFechamentoAsync(
        Guid fechamentoFolhaId, CancellationToken ct = default) =>
        await _context.Contracheques
            .Include("_itens")
            .Where(c => c.FechamentoFolhaId == fechamentoFolhaId)
            .OrderBy(c => c.FuncionarioId)
            .ToListAsync(ct);

    public async Task<bool> ExistePorFuncionarioEFechamentoAsync(
        Guid funcionarioId, Guid fechamentoFolhaId, CancellationToken ct = default) =>
        await _context.Contracheques
            .AnyAsync(c => c.FuncionarioId == funcionarioId
                        && c.FechamentoFolhaId == fechamentoFolhaId, ct);

    public async Task AdicionarAsync(Contracheque contracheque, CancellationToken ct = default) =>
        await _context.Contracheques.AddAsync(contracheque, ct);

    public void Atualizar(Contracheque contracheque) =>
        _context.Contracheques.Update(contracheque);
}
