namespace Application.Interfaces;

/// <summary>
/// Abstrai o controle de transação do banco de dados.
/// Implementado na camada Infrastructure (EF Core DbContext).
/// Permite que Use Cases commitam múltiplas operações em uma única transação
/// sem depender de nenhum framework.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persiste todas as alterações pendentes no banco de dados.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}
