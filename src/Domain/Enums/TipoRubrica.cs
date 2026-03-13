namespace Domain.Enums;

/// <summary>
/// Classifica cada rubrica (linha) do contracheque como provento ou desconto.
/// Proventos somam; descontos são subtraídos do salário bruto.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public enum TipoRubrica
{
    // ─── Proventos ─────────────────────────────────────────────────────────
    /// <summary>Salário base contratual do mês.</summary>
    SalarioBase = 1,

    /// <summary>Horas extras a 50% (dias úteis).</summary>
    HoraExtra50 = 2,

    /// <summary>Horas extras a 100% (domingos e feriados).</summary>
    HoraExtra100 = 3,

    /// <summary>Adicional noturno (22h–05h CLT art. 73).</summary>
    AdicionalNoturno = 4,

    // ─── Descontos Legais ──────────────────────────────────────────────────
    /// <summary>Contribuição previdenciária (INSS) — tabela progressiva 2024.</summary>
    DescontoInss = 10,

    /// <summary>Imposto de Renda Retido na Fonte — tabela progressiva 2024.</summary>
    DescontoIrrf = 11,

    /// <summary>Contribuição ao FGTS (8% do bruto — não desconta do líquido, apenas informativo).</summary>
    FgtsInformativo = 12,

    /// <summary>Desconto por atraso ou ausência apurado no fechamento.</summary>
    DescontoAtraso = 13
}
