namespace Domain.ValueObjects;

/// <summary>
/// Define o turno contratual do funcionário (horário de entrada, saída e duração do intervalo).
/// Imutável por design (record).
/// </summary>
public sealed record TurnoTrabalho
{
    public TimeOnly HoraEntrada { get; }
    public TimeOnly HoraSaida { get; }
    public TimeSpan DuracaoIntervalo { get; }

    /// <summary>Carga horária diária líquida (sem intervalo).</summary>
    public TimeSpan CargaHorariaDiaria =>
        (HoraSaida - HoraEntrada) - DuracaoIntervalo;

    public TurnoTrabalho(TimeOnly horaEntrada, TimeOnly horaSaida, TimeSpan duracaoIntervalo)
    {
        if (horaSaida <= horaEntrada)
            throw new ArgumentException(
                "A hora de saída deve ser posterior à hora de entrada.",
                nameof(horaSaida));

        if (duracaoIntervalo < TimeSpan.Zero)
            throw new ArgumentException(
                "A duração do intervalo não pode ser negativa.",
                nameof(duracaoIntervalo));

        HoraEntrada = horaEntrada;
        HoraSaida = horaSaida;
        DuracaoIntervalo = duracaoIntervalo;
    }
}
