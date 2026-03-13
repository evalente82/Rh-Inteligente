namespace Application.DTOs;

/// <summary>
/// Dados de entrada para acionar a análise de IA sobre os registros de ponto
/// de um funcionário em um determinado período.
/// </summary>
public sealed record AnalisarRegistrosInputDTO(
    /// <summary>Identificador do tenant.</summary>
    Guid EmpresaId,

    /// <summary>Funcionário cujos registros serão analisados.</summary>
    Guid FuncionarioId,

    /// <summary>Início do período a ser auditado.</summary>
    DateOnly PeriodoInicio,

    /// <summary>Fim do período a ser auditado.</summary>
    DateOnly PeriodoFim
);
