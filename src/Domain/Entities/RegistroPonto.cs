using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Representa uma batida de ponto bruta de um funcionário.
/// POCO puro — sem dependências externas.
/// </summary>
public sealed class RegistroPonto
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant. Obrigatório (Regra 5 - Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public Guid FuncionarioId { get; private set; }

    public DateTime DataHoraBatida { get; private set; }
    public TipoBatida TipoBatida { get; private set; }

    /// <summary>
    /// Origem da batida: "REP", "App", "Manual", etc.
    /// Mantido como string para extensibilidade sem quebrar o domínio.
    /// </summary>
    public string Origem { get; private set; } = string.Empty;

    /// <summary>Indica se a batida foi inserida manualmente pelo gestor (requer justificativa).</summary>
    public bool LancamentoManual { get; private set; }

    public string? Justificativa { get; private set; }

    private RegistroPonto() { }

    /// <summary>
    /// Cria um novo registro de ponto validando todas as invariantes do domínio.
    /// </summary>
    public static RegistroPonto Criar(
        Guid empresaId,
        Guid funcionarioId,
        DateTime dataHoraBatida,
        TipoBatida tipoBatida,
        string origem,
        bool lancamentoManual = false,
        string? justificativa = null)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (funcionarioId == Guid.Empty)
            throw new ArgumentException("FuncionarioId não pode ser vazio.", nameof(funcionarioId));

        ArgumentException.ThrowIfNullOrWhiteSpace(origem);

        if (lancamentoManual && string.IsNullOrWhiteSpace(justificativa))
            throw new InvalidOperationException(
                "Lançamentos manuais exigem uma justificativa preenchida.");

        return new RegistroPonto
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FuncionarioId = funcionarioId,
            DataHoraBatida = dataHoraBatida,
            TipoBatida = tipoBatida,
            Origem = origem.Trim(),
            LancamentoManual = lancamentoManual,
            Justificativa = justificativa?.Trim()
        };
    }
}
