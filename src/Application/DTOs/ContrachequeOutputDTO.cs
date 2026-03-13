namespace Application.DTOs;

/// <summary>
/// Projeção de uma rubrica (linha) do contracheque.
/// </summary>
public sealed record ItemContrachequeOutputDTO(
    string Tipo,
    string Descricao,
    decimal Valor,
    bool EhDesconto);

/// <summary>
/// Projeção completa de um contracheque para exibição no frontend/relatórios.
/// </summary>
public sealed record ContrachequeOutputDTO(
    Guid Id,
    Guid EmpresaId,
    Guid FechamentoFolhaId,
    Guid FuncionarioId,
    string Competencia,
    decimal SalarioBruto,
    decimal TotalDescontos,
    decimal SalarioLiquido,
    decimal FgtsPatronal,
    DateTime GeradoEm,
    IReadOnlyCollection<ItemContrachequeOutputDTO> Itens);
