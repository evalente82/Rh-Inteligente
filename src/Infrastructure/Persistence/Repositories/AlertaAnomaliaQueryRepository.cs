using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de leitura de AlertaAnomalia para o Dashboard de Risco (M5).
/// AsNoTracking — otimizado para queries analíticas.
/// </summary>
internal sealed class AlertaAnomaliaQueryRepository : IAlertaAnomaliaQueryRepository
{
    private readonly RhInteligenteDbContext _context;

    public AlertaAnomaliaQueryRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AlertaAnomalia>> ListarPorPeriodoAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default)
    {
        // O Global Query Filter já aplica o filtro por EmpresaId
        return await _context.AlertasAnomalia
            .AsNoTracking()
            .Where(a =>
                a.DataReferencia >= periodoInicio &&
                a.DataReferencia <= periodoFim)
            .ToListAsync(ct);
    }
}
