using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 do repositório de <see cref="Admissao"/>.
/// O Global Query Filter por EmpresaId no DbContext garante o isolamento multi-tenant.
/// </summary>
internal sealed class AdmissaoRepository : IAdmissaoRepository
{
    private readonly RhInteligenteDbContext _context;

    public AdmissaoRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<Admissao?> ObterAdmissaoAtivaAsync(
        Guid funcionarioId,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Admissoes
            .FirstOrDefaultAsync(
                a => a.FuncionarioId == funcionarioId && a.DataDemissao == null,
                cancellationToken);
    }

    public async Task AdicionarAsync(
        Admissao admissao,
        CancellationToken cancellationToken = default)
    {
        await _context.Admissoes.AddAsync(admissao, cancellationToken);
    }

    public void Atualizar(Admissao admissao)
    {
        _context.Admissoes.Update(admissao);
    }
}
