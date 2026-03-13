using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para o agregado <see cref="Contracheque"/>.
/// Implementado na camada Infrastructure (EF Core).
/// </summary>
public interface IContrachequeRepository
{
    Task<Contracheque?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Contracheque>> ListarPorFechamentoAsync(Guid fechamentoFolhaId, CancellationToken ct = default);
    Task<bool> ExistePorFuncionarioEFechamentoAsync(Guid funcionarioId, Guid fechamentoFolhaId, CancellationToken ct = default);
    Task AdicionarAsync(Contracheque contracheque, CancellationToken ct = default);
    void Atualizar(Contracheque contracheque);
}
