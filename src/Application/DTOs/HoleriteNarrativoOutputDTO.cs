namespace Application.DTOs;

/// <summary>
/// DTO de saída do holerite narrativo gerado pela IA (Gemini 2.5 Flash).
/// Texto gerado descrevendo o resumo do período de trabalho do funcionário,
/// horas extras, anomalias e projeção salarial.
/// </summary>
public sealed record HoleriteNarrativoOutputDTO(
    Guid FuncionarioId,
    Guid EmpresaId,
    string NomeFuncionario,
    string Periodo,
    string TextoNarrativo,
    int TotalAnomalias,
    decimal TotalHorasExtras,
    DateTime GeradoEm);
