namespace Application.DTOs;

/// <summary>Entrada para solicitar o fechamento de um período de folha.</summary>
public sealed record FecharFolhaInputDTO(
    Guid EmpresaId,
    DateOnly PeriodoInicio,
    DateOnly PeriodoFim
);
