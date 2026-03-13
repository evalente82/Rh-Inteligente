using Application.DTOs;

namespace Application.Interfaces;

/// <summary>
/// Contrato de consulta de anomalias para o dashboard de risco.
/// Operação de leitura pura — não modifica estado.
/// </summary>
public interface IAlertaAnomaliaQueryRepository
{
    Task<IReadOnlyList<Domain.Entities.AlertaAnomalia>> ListarPorPeriodoAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default);
}
