using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

internal sealed class FechamentoFolhaRepository : IFechamentoFolhaRepository
{
    private readonly RhInteligenteDbContext _context;

    public FechamentoFolhaRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<FechamentoFolha?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.FechamentosFolha
            .FirstOrDefaultAsync(f => f.Id == id, ct);

    public async Task<FechamentoFolha?> ObterAbertoPorPeriodoAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default) =>
        await _context.FechamentosFolha
            .FirstOrDefaultAsync(
                f => f.EmpresaId == empresaId
                     && f.PeriodoInicio == periodoInicio
                     && f.PeriodoFim == periodoFim,
                ct);

    public async Task AdicionarAsync(FechamentoFolha fechamento, CancellationToken ct = default) =>
        await _context.FechamentosFolha.AddAsync(fechamento, ct);

    public void Atualizar(FechamentoFolha fechamento) =>
        _context.FechamentosFolha.Update(fechamento);
}
