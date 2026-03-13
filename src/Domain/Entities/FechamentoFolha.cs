using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Agregado raiz que representa o fechamento de um período de folha de ponto.
/// Consolida horas extras, descontos e a narrativa gerada pela IA.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed class FechamentoFolha
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant. Obrigatório (Regra 5 - Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public DateOnly PeriodoInicio { get; private set; }
    public DateOnly PeriodoFim { get; private set; }

    public StatusFolha Status { get; private set; }

    /// <summary>Total de horas extras acumuladas no período (em horas decimais).</summary>
    public decimal TotalHorasExtras { get; private set; }

    /// <summary>Total de descontos por faltas/atrasos no período (em horas decimais).</summary>
    public decimal TotalDescontos { get; private set; }

    /// <summary>Número de anomalias críticas encontradas no período.</summary>
    public int TotalAnomaliasCriticas { get; private set; }

    /// <summary>Narrativa gerada pela IA resumindo o período (preenchida ao fechar).</summary>
    public string? RelatorioNarrativo { get; private set; }

    public DateTime CriadaEm { get; private set; }
    public DateTime? FechadaEm { get; private set; }
    public DateTime? AprovadaEm { get; private set; }

    private FechamentoFolha() { }

    /// <summary>
    /// Abre um novo período de fechamento para o tenant.
    /// </summary>
    public static FechamentoFolha Abrir(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (periodoFim <= periodoInicio)
            throw new ArgumentException("PeriodoFim deve ser posterior ao PeriodoInicio.");

        return new FechamentoFolha
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            PeriodoInicio = periodoInicio,
            PeriodoFim = periodoFim,
            Status = StatusFolha.Aberta,
            TotalHorasExtras = 0,
            TotalDescontos = 0,
            TotalAnomaliasCriticas = 0,
            CriadaEm = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza apenas o relatório narrativo gerado pela IA (pode ser chamado após fechar).
    /// </summary>
    public void AtualizarRelatorio(string relatorioNarrativo)
    {
        if (Status == StatusFolha.Aprovada)
            throw new InvalidOperationException("Não é possível alterar a narrativa de uma folha aprovada.");

        ArgumentException.ThrowIfNullOrWhiteSpace(relatorioNarrativo);
        RelatorioNarrativo = relatorioNarrativo;
    }

    /// <summary>
    /// Consolida os totais calculados e gera o relatório narrativo, transitando para Fechada.
    /// </summary>
    public void Fechar(
        decimal totalHorasExtras,
        decimal totalDescontos,
        int totalAnomaliasCriticas,
        string relatorioNarrativo)
    {
        if (Status != StatusFolha.Aberta)
            throw new InvalidOperationException($"Não é possível fechar uma folha com status '{Status}'.");

        if (totalHorasExtras < 0)
            throw new ArgumentException("TotalHorasExtras não pode ser negativo.", nameof(totalHorasExtras));

        if (totalDescontos < 0)
            throw new ArgumentException("TotalDescontos não pode ser negativo.", nameof(totalDescontos));

        if (totalAnomaliasCriticas < 0)
            throw new ArgumentException("TotalAnomaliasCriticas não pode ser negativo.", nameof(totalAnomaliasCriticas));

        ArgumentException.ThrowIfNullOrWhiteSpace(relatorioNarrativo);

        TotalHorasExtras = totalHorasExtras;
        TotalDescontos = totalDescontos;
        TotalAnomaliasCriticas = totalAnomaliasCriticas;
        RelatorioNarrativo = relatorioNarrativo;
        Status = StatusFolha.Fechada;
        FechadaEm = DateTime.UtcNow;
    }

    /// <summary>
    /// Aprova a folha fechada — estado terminal, imutável para edições posteriores.
    /// </summary>
    public void Aprovar()
    {
        if (Status != StatusFolha.Fechada)
            throw new InvalidOperationException($"Somente folhas com status 'Fechada' podem ser aprovadas. Status atual: '{Status}'.");

        Status = StatusFolha.Aprovada;
        AprovadaEm = DateTime.UtcNow;
    }
}
