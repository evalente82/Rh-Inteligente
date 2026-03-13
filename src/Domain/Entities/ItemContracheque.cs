using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// Representa uma rubrica (linha) do contracheque: descrição, valor e se é provento ou desconto.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed class ItemContracheque
{
    public Guid Id { get; private set; }
    public Guid ContrachequeId { get; private set; }

    /// <summary>Classificação da rubrica (TipoRubrica).</summary>
    public TipoRubrica Tipo { get; private set; }

    /// <summary>Descrição legível, ex: "INSS (7,5%)".</summary>
    public string Descricao { get; private set; } = string.Empty;

    /// <summary>Valor sempre positivo; o sinal é determinado pelo Tipo.</summary>
    public decimal Valor { get; private set; }

    /// <summary>
    /// Indica se o item reduz o salário líquido.
    /// FgtsInformativo é encargo patronal — aparece no holerite mas NÃO desconta do líquido.
    /// </summary>
    public bool EhDesconto => Tipo is TipoRubrica.DescontoInss
                                   or TipoRubrica.DescontoIrrf
                                   or TipoRubrica.DescontoAtraso;

    // Navegação para o pai — usada pelo EF Core para mapear o relacionamento sem shadow property.
    // Não é exposta ao código de negócio (private set).
    public Contracheque? Contracheque { get; private set; }

    private ItemContracheque() { }

    internal static ItemContracheque Criar(Guid contrachequeId, TipoRubrica tipo, string descricao, decimal valor)
    {
        if (contrachequeId == Guid.Empty)
            throw new ArgumentException("ContrachequeId não pode ser vazio.", nameof(contrachequeId));

        ArgumentException.ThrowIfNullOrWhiteSpace(descricao);

        if (valor < 0)
            throw new ArgumentException("Valor do item não pode ser negativo.", nameof(valor));

        return new ItemContracheque
        {
            Id = Guid.NewGuid(),
            ContrachequeId = contrachequeId,
            Tipo = tipo,
            Descricao = descricao.Trim(),
            Valor = valor
        };
    }
}
