using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 do repositório de <see cref="RegistroPonto"/>.
/// O Global Query Filter por EmpresaId no DbContext garante isolamento multi-tenant automaticamente.
/// </summary>
internal sealed class RegistroPontoRepository : IRegistroPontoRepository
{
    private readonly RhInteligenteDbContext _context;

    public RegistroPontoRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegistroPonto>> ListarPorFuncionarioEDiaAsync(
        Guid funcionarioId,
        Guid empresaId,
        DateOnly data,
        CancellationToken cancellationToken = default)
    {
        // Converte DateOnly para intervalo DateTime para uso com o campo DataHoraBatida
        var inicio = data.ToDateTime(TimeOnly.MinValue);
        var fim = data.ToDateTime(TimeOnly.MaxValue);

        return await _context.RegistrosPonto
            .AsNoTracking()
            .Where(r => r.FuncionarioId == funcionarioId
                        && r.DataHoraBatida >= inicio
                        && r.DataHoraBatida <= fim)
            .OrderBy(r => r.DataHoraBatida)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RegistroPonto>> ListarPorPeriodoAsync(
        Guid funcionarioId,
        Guid empresaId,
        DateOnly dataInicio,
        DateOnly dataFim,
        CancellationToken cancellationToken = default)
    {
        var inicio = dataInicio.ToDateTime(TimeOnly.MinValue);
        var fim = dataFim.ToDateTime(TimeOnly.MaxValue);

        return await _context.RegistrosPonto
            .AsNoTracking()
            .Where(r => r.FuncionarioId == funcionarioId
                        && r.DataHoraBatida >= inicio
                        && r.DataHoraBatida <= fim)
            .OrderBy(r => r.DataHoraBatida)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        RegistroPonto registro,
        CancellationToken cancellationToken = default)
    {
        await _context.RegistrosPonto.AddAsync(registro, cancellationToken);
    }

    public async Task AdicionarLoteAsync(
        IEnumerable<RegistroPonto> registros,
        CancellationToken cancellationToken = default)
    {
        await _context.RegistrosPonto.AddRangeAsync(registros, cancellationToken);
    }
}
