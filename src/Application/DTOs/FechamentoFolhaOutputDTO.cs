namespace Application.DTOs;

/// <summary>Projeção de saída de um fechamento de folha.</summary>
public sealed record FechamentoFolhaOutputDTO(
    Guid Id,
    Guid EmpresaId,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim,
    string Status,
    decimal TotalHorasExtras,
    decimal TotalDescontos,
    int TotalAnomaliasCriticas,
    string? RelatorioNarrativo,
    DateTime CriadaEm,
    DateTime? FechadaEm,
    DateTime? AprovadaEm
);
