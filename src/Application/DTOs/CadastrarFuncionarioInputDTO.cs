namespace Application.DTOs;

/// <summary>
/// Dados de entrada para cadastrar um novo funcionário.
/// </summary>
public sealed record CadastrarFuncionarioInputDTO(
    Guid EmpresaId,
    string Nome,
    string Cpf,
    string Matricula,
    DateTime DataAdmissao,
    /// <summary>Hora de entrada no formato "HH:mm" ex: "08:00".</summary>
    string HoraEntrada,
    /// <summary>Hora de saída no formato "HH:mm" ex: "17:00".</summary>
    string HoraSaida,
    /// <summary>Duração do intervalo de almoço em minutos (ex: 60).</summary>
    int IntervaloAlmocoMinutos
);
