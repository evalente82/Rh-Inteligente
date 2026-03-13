using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para o agregado <see cref="Funcionario"/>.
/// Implementado na camada Infrastructure (EF Core).
/// </summary>
public interface IFuncionarioRepository
{
    Task<Funcionario?> ObterPorIdAsync(Guid id, Guid empresaId, CancellationToken cancellationToken = default);
    Task<Funcionario?> ObterPorMatriculaAsync(string matricula, Guid empresaId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Funcionario>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Funcionario funcionario, CancellationToken cancellationToken = default);
    void Atualizar(Funcionario funcionario);
}
