using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Agregado raiz que representa o colaborador vinculado a uma empresa (tenant).
/// POCO puro — sem dependências externas.
/// </summary>
public sealed class Funcionario
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant. Obrigatório (Regra 5 - Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public string Nome { get; private set; } = string.Empty;

    /// <summary>CPF validado como Value Object (dígito verificador).</summary>
    public Cpf Cpf { get; private set; } = default!;

    public string Matricula { get; private set; } = string.Empty;
    public DateTime DataAdmissao { get; private set; }
    public DateTime? DataDemissao { get; private set; }

    /// <summary>Turno contratual do funcionário (Value Object imutável).</summary>
    public TurnoTrabalho TurnoContratual { get; private set; } = default!;

    /// <summary>Indica se o funcionário está atualmente ativo.</summary>
    public bool Ativo => DataDemissao is null || DataDemissao > DateTime.UtcNow;

    private readonly List<RegistroPonto> _registrosPonto = [];
    public IReadOnlyCollection<RegistroPonto> RegistrosPonto => _registrosPonto.AsReadOnly();

    private readonly List<Admissao> _admissoes = [];
    public IReadOnlyCollection<Admissao> Admissoes => _admissoes.AsReadOnly();

    /// <summary>Admissão ativa atual (pode ser null se ainda não admitido formalmente).</summary>
    public Admissao? AdmissaoAtiva => _admissoes.FirstOrDefault(a => a.Ativa);

    // Construtor privado — força uso do factory method
    private Funcionario() { }

    /// <summary>
    /// Cria um novo funcionário validando todas as invariantes do domínio.
    /// </summary>
    public static Funcionario Criar(
        Guid empresaId,
        string nome,
        Cpf cpf,
        string matricula,
        DateTime dataAdmissao,
        TurnoTrabalho turnoContratual)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentNullException.ThrowIfNull(cpf);
        ArgumentException.ThrowIfNullOrWhiteSpace(matricula);
        ArgumentNullException.ThrowIfNull(turnoContratual);

        return new Funcionario
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Nome = nome.Trim(),
            Cpf = cpf,
            Matricula = matricula.Trim(),
            DataAdmissao = dataAdmissao,
            TurnoContratual = turnoContratual
        };
    }

    /// <summary>Registra a demissão do funcionário.</summary>
    public void Demitir(DateTime dataDemissao)
    {
        if (dataDemissao < DataAdmissao)
            throw new InvalidOperationException(
                "A data de demissão não pode ser anterior à data de admissão.");

        DataDemissao = dataDemissao;
    }

    /// <summary>Atualiza o turno contratual (ex: mudança de horário por acordo).</summary>
    public void AtualizarTurno(TurnoTrabalho novoTurno)
    {
        ArgumentNullException.ThrowIfNull(novoTurno);
        TurnoContratual = novoTurno;
    }
}

