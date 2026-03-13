using Domain.Entities;

namespace Application.Interfaces;

public interface IFechamentoFolhaRepository
{
    Task<FechamentoFolha?> ObterPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Retorna o fechamento aberto do período, se existir.</summary>
    Task<FechamentoFolha?> ObterAbertoPorPeriodoAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default);

    Task AdicionarAsync(FechamentoFolha fechamento, CancellationToken ct = default);
    void Atualizar(FechamentoFolha fechamento);
}
