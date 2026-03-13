using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Representa o contracheque (holerite de pagamento) de um funcionário em um fechamento de folha.
/// Contém todas as rubricas de proventos e descontos, além dos totais calculados.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed class Contracheque
{
    public Guid Id { get; private set; }

    /// <summary>Identificador do tenant (Regra 5 — Multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public Guid FechamentoFolhaId { get; private set; }
    public Guid FuncionarioId { get; private set; }

    /// <summary>Competência no formato "MM/YYYY", ex: "03/2026".</summary>
    public string Competencia { get; private set; } = string.Empty;

    /// <summary>Salário bruto = SalarioBase + todos os proventos adicionais.</summary>
    public decimal SalarioBruto { get; private set; }

    /// <summary>Total dos descontos (INSS + IRRF + faltas/atrasos).</summary>
    public decimal TotalDescontos { get; private set; }

    /// <summary>Salário líquido = SalarioBruto - TotalDescontos.</summary>
    public decimal SalarioLiquido => SalarioBruto - TotalDescontos;

    /// <summary>FGTS do mês (8% do bruto) — encargo patronal, informativo.</summary>
    public decimal FgtsPatronal { get; private set; }

    /// <summary>Data/hora UTC em que o contracheque foi gerado.</summary>
    public DateTime GeradoEm { get; private set; }

    private readonly List<ItemContracheque> _itens = [];

    /// <summary>
    /// Rubricas do contracheque.
    /// EF Core 8 acessa via backing field _itens; o código de negócio usa Add via AdicionarItem().
    /// </summary>
    public IReadOnlyCollection<ItemContracheque> Itens => _itens.AsReadOnly();

    private Contracheque() { }

    /// <summary>
    /// Cria um novo contracheque vazio pronto para receber rubricas.
    /// </summary>
    public static Contracheque Criar(
        Guid empresaId,
        Guid fechamentoFolhaId,
        Guid funcionarioId,
        string competencia)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));
        if (fechamentoFolhaId == Guid.Empty)
            throw new ArgumentException("FechamentoFolhaId não pode ser vazio.", nameof(fechamentoFolhaId));
        if (funcionarioId == Guid.Empty)
            throw new ArgumentException("FuncionarioId não pode ser vazio.", nameof(funcionarioId));

        ArgumentException.ThrowIfNullOrWhiteSpace(competencia);

        return new Contracheque
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            FechamentoFolhaId = fechamentoFolhaId,
            FuncionarioId = funcionarioId,
            Competencia = competencia.Trim(),
            GeradoEm = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Adiciona uma rubrica ao contracheque e recalcula os totais imediatamente.
    /// </summary>
    public void AdicionarItem(TipoRubrica tipo, string descricao, decimal valor)
    {
        var item = ItemContracheque.Criar(Id, tipo, descricao, valor);
        _itens.Add(item);
        RecalcularTotais();
    }

    // ─── Invariante: totais sempre consistentes com os itens ─────────────────
    private void RecalcularTotais()
    {
        SalarioBruto = _itens
            .Where(i => !i.EhDesconto && i.Tipo != TipoRubrica.FgtsInformativo)
            .Sum(i => i.Valor);

        TotalDescontos = _itens
            .Where(i => i.EhDesconto)
            .Sum(i => i.Valor);

        FgtsPatronal = _itens
            .Where(i => i.Tipo == TipoRubrica.FgtsInformativo)
            .Sum(i => i.Valor);
    }
}
