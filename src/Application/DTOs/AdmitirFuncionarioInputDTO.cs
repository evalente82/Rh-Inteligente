using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Dados de entrada para formalizar a admissão de um funcionário já cadastrado.
/// </summary>
public sealed record AdmitirFuncionarioInputDTO(
    Guid EmpresaId,
    Guid FuncionarioId,
    string Cargo,
    decimal SalarioBase,
    RegimeContratacao Regime,
    DateOnly DataAdmissao,
    // Endereço
    string Logradouro,
    string NumeroEndereco,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string? Complemento = null
);
