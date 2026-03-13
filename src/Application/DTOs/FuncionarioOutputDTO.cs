using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Projeção de saída de um funcionário. Nunca expõe a entidade de domínio diretamente.
/// </summary>
public sealed record FuncionarioOutputDTO(
    Guid Id,
    Guid EmpresaId,
    string Nome,
    string Cpf,
    string Matricula,
    bool Ativo,
    DateTime DataAdmissao,
    DateTime? DataDemissao,
    // Turno
    string HoraEntrada,
    string HoraSaida,
    int IntervaloAlmocoMinutos,
    // Admissão ativa (null se ainda não admitido formalmente)
    string? Cargo,
    decimal? SalarioBase,
    RegimeContratacao? Regime
);
