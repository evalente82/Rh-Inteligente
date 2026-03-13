namespace Application.DTOs;

/// <summary>
/// Entrada para geração dos contracheques de todos os funcionários de um fechamento.
/// </summary>
public sealed record GerarContrachequeInputDTO(
    Guid EmpresaId,
    Guid FechamentoFolhaId);
