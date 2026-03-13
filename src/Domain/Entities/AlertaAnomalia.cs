using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Representa uma anomalia detectada pela IA após auditar os registros de ponto.
/// É o resultado central do Módulo 1 — gerado pelo AnalisarRegistrosComIaUseCase.
/// POCO puro — sem dependências externas.
/// </summary>
public sealed class AlertaAnomalia
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant. Obrigatório (Regra 5 - Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public TipoAnomalia TipoAnomalia { get; private set; }

    /// <summary>Data de referência da anomalia (dia do ocorrido).</summary>
    public DateOnly DataReferencia { get; private set; }

    /// <summary>Descrição gerada pela IA explicando a divergência encontrada.</summary>
    public string Descricao { get; private set; } = string.Empty;

    /// <summary>
    /// Nível de gravidade do alerta:
    /// 1 = Informativo | 2 = Atenção | 3 = Crítico
    /// </summary>
    public int Gravidade { get; private set; }

    public DateTime GeradoEm { get; private set; }

    /// <summary>Indica se o gestor já tomou ciência e encerrou o alerta.</summary>
    public bool Resolvido { get; private set; }

    public DateTime? ResolvidoEm { get; private set; }

    private AlertaAnomalia() { }

    /// <summary>
    /// Cria um novo alerta de anomalia validando todas as invariantes do domínio.
    /// </summary>
    public static AlertaAnomalia Criar(
        Guid empresaId,
        Guid funcionarioId,
        TipoAnomalia tipoAnomalia,
        DateOnly dataReferencia,
        string descricao,
        int gravidade)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (funcionarioId == Guid.Empty)
            throw new ArgumentException("FuncionarioId não pode ser vazio.", nameof(funcionarioId));

        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);

        if (gravidade is < 1 or > 3)
            throw new ArgumentOutOfRangeException(
                nameof(gravidade),
                "Gravidade deve ser entre 1 (informativo) e 3 (crítico).");

        return new AlertaAnomalia
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FuncionarioId = funcionarioId,
            TipoAnomalia = tipoAnomalia,
            DataReferencia = dataReferencia,
            Descricao = descricao.Trim(),
            Gravidade = gravidade,
            GeradoEm = DateTime.UtcNow,
            Resolvido = false
        };
    }

    /// <summary>
    /// Marca o alerta como resolvido pelo gestor.
    /// </summary>
    public void MarcarComoResolvido()
    {
        if (Resolvido)
            throw new InvalidOperationException("Este alerta já foi resolvido anteriormente.");

        Resolvido = true;
        ResolvidoEm = DateTime.UtcNow;
    }
}
