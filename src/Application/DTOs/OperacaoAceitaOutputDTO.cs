namespace Application.DTOs;

/// <summary>
/// Resposta padrão de operações assíncronas que retornam 202 Accepted.
/// Contém o identificador do job/processo para rastreamento pelo cliente.
/// </summary>
public sealed record OperacaoAceitaOutputDTO(
    /// <summary>Identificador único do processo enfileirado para rastreamento.</summary>
    Guid ProcessoId,

    /// <summary>Mensagem descritiva para exibição no Frontend.</summary>
    string Mensagem
);
