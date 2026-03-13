namespace Domain.ValueObjects;

/// <summary>
/// Representa um intervalo de tempo entre dois instantes (ex: saída e retorno do almoço).
/// Imutável por design (record).
/// </summary>
public sealed record IntervaloTempo
{
    /// <summary>Duração mínima de intervalo exigida pela CLT (1 hora para jornadas acima de 6h).</summary>
    public static readonly TimeSpan MinimoLegalClt = TimeSpan.FromHours(1);

    public DateTime Inicio { get; }
    public DateTime Fim { get; }

    public TimeSpan Duracao => Fim - Inicio;

    public IntervaloTempo(DateTime inicio, DateTime fim)
    {
        if (fim <= inicio)
            throw new ArgumentException(
                "O fim do intervalo deve ser posterior ao início.",
                nameof(fim));

        Inicio = inicio;
        Fim = fim;
    }

    /// <summary>
    /// Verifica se este intervalo respeita a duração mínima exigida.
    /// </summary>
    public bool RespeitaMinimo(TimeSpan duracaoMinima) => Duracao >= duracaoMinima;
}
