namespace Application.DTOs;

/// <summary>Dados da empresa criada retornados após onboarding.</summary>
public sealed record EmpresaOutputDTO(
    Guid Id,
    string NomeFantasia,
    string CnpjFormatado,
    string EmailContato,
    DateTime CriadaEm
);
