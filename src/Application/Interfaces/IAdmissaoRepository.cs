using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para a entidade <see cref="Admissao"/>.
/// Implementado na camada Infrastructure (EF Core).
/// </summary>
public interface IAdmissaoRepository
{
    Task<Admissao?> ObterAdmissaoAtivaAsync(Guid funcionarioId, Guid empresaId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Admissao admissao, CancellationToken cancellationToken = default);
    void Atualizar(Admissao admissao);
}
