using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para <see cref="RegistroPonto"/>.
/// Implementado na camada Infrastructure (EF Core).
/// </summary>
public interface IRegistroPontoRepository
{
    Task<IEnumerable<RegistroPonto>> ListarPorFuncionarioEDiaAsync(
        Guid funcionarioId,
        Guid empresaId,
        DateOnly data,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<RegistroPonto>> ListarPorPeriodoAsync(
        Guid funcionarioId,
        Guid empresaId,
        DateOnly dataInicio,
        DateOnly dataFim,
        CancellationToken cancellationToken = default);

    Task AdicionarAsync(RegistroPonto registro, CancellationToken cancellationToken = default);
    Task AdicionarLoteAsync(IEnumerable<RegistroPonto> registros, CancellationToken cancellationToken = default);
}
