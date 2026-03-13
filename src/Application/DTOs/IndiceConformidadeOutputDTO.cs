namespace Application.DTOs;

/// <summary>
/// Índice de conformidade trabalhista calculado para o período.
/// 100% = zero anomalias críticas. Abaixo de 70% = risco alto.
/// </summary>
public sealed record IndiceConformidadeOutputDTO(
    Guid EmpresaId,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim,
    /// <summary>Índice de 0 a 100. 100 = conformidade total.</summary>
    double IndiceConformidade,
    string Classificacao,  // "Verde" | "Amarelo" | "Vermelho"
    int TotalFuncionariosAtivos,
    int FuncionariosComAlertas,
    int FuncionariosSemAlertas
);
