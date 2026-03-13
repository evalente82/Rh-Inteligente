using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Resumo agregado de anomalias por tipo para o dashboard de risco.
/// </summary>
public sealed record AnomaliasPorTipoDTO(
    TipoAnomalia Tipo,
    string Descricao,
    int Total,
    int Criticas,
    int Atencao,
    int Informativas
);

/// <summary>
/// Funcionário com maior concentração de alertas — usado no ranking do dashboard.
/// </summary>
public sealed record FuncionarioRiscoDTO(
    Guid FuncionarioId,
    string NomeFuncionario,
    int TotalAlertas,
    int AlertasCriticos
);

/// <summary>
/// DTO de saída completo para o Dashboard de Risco Trabalhista.
/// Agrega alertas de anomalia de todo o tenant no período informado.
/// </summary>
public sealed record DashboardRiscoOutputDTO(
    Guid EmpresaId,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim,
    int TotalAlertas,
    int TotalCriticos,
    int TotalAtencao,
    int TotalInformativos,
    int TotalResolvidos,
    IReadOnlyList<AnomaliasPorTipoDTO> AnomaliasPorTipo,
    IReadOnlyList<FuncionarioRiscoDTO> TopFuncionariosRisco
);
