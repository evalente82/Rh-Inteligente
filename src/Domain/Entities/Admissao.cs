using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa o vínculo formal de um colaborador com a empresa (admissão/demissão).
/// Contém as informações necessárias para o eSocial e cálculo de folha.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed class Admissao
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant. Obrigatório (Regra 5 - Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public string Cargo { get; private set; } = string.Empty;

    /// <summary>Salário base em reais (R$). Sempre positivo.</summary>
    public decimal SalarioBase { get; private set; }

    public RegimeContratacao Regime { get; private set; }

    public DateOnly DataAdmissao { get; private set; }

    public DateOnly? DataDemissao { get; private set; }

    /// <summary>Endereço do colaborador na data de admissão (para registro eSocial).</summary>
    public Endereco EnderecoResidencial { get; private set; } = default!;

    /// <summary>Admissão ativa enquanto não há data de demissão.</summary>
    public bool Ativa => DataDemissao is null;

    // Construtor privado — força uso do factory method
    private Admissao() { }

    /// <summary>
    /// Cria um novo registro de admissão validando invariantes do domínio.
    /// </summary>
    public static Admissao Criar(
        Guid empresaId,
        Guid funcionarioId,
        string cargo,
        decimal salarioBase,
        RegimeContratacao regime,
        DateOnly dataAdmissao,
        Endereco enderecoResidencial)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (funcionarioId == Guid.Empty)
            throw new ArgumentException("FuncionarioId não pode ser vazio.", nameof(funcionarioId));

        ArgumentException.ThrowIfNullOrWhiteSpace(cargo);
        ArgumentNullException.ThrowIfNull(enderecoResidencial);

        if (salarioBase <= 0)
            throw new ArgumentException("Salário base deve ser maior que zero.", nameof(salarioBase));

        if (dataAdmissao > DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30))
            throw new ArgumentException(
                "Data de admissão não pode ser superior a 30 dias no futuro.", nameof(dataAdmissao));

        return new Admissao
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FuncionarioId = funcionarioId,
            Cargo = cargo.Trim(),
            SalarioBase = salarioBase,
            Regime = regime,
            DataAdmissao = dataAdmissao,
            EnderecoResidencial = enderecoResidencial
        };
    }

    /// <summary>Registra o desligamento do colaborador.</summary>
    public void Demitir(DateOnly dataDemissao)
    {
        if (!Ativa)
            throw new InvalidOperationException("Esta admissão já foi encerrada.");

        if (dataDemissao < DataAdmissao)
            throw new InvalidOperationException(
                "A data de demissão não pode ser anterior à data de admissão.");

        DataDemissao = dataDemissao;
    }

    /// <summary>Aplica reajuste salarial (sempre positivo).</summary>
    public void ReajustarSalario(decimal novoSalario)
    {
        if (novoSalario <= 0)
            throw new ArgumentException("Novo salário deve ser maior que zero.", nameof(novoSalario));

        if (!Ativa)
            throw new InvalidOperationException("Não é possível reajustar salário de uma admissão encerrada.");

        SalarioBase = novoSalario;
    }

    /// <summary>Atualiza o endereço residencial (mudança de domicílio).</summary>
    public void AtualizarEndereco(Endereco novoEndereco)
    {
        ArgumentNullException.ThrowIfNull(novoEndereco);
        EnderecoResidencial = novoEndereco;
    }
}
