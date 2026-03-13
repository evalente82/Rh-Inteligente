using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Domain Service responsável por calcular horas extras e noturnas de um funcionário
/// com base nos registros de ponto do dia versus o turno contratual.
///
/// Regras CLT aplicadas:
///   - HE diurna: acréscimo de 50% sobre a hora normal.
///   - HE em feriados/domingos: acréscimo de 100% sobre a hora normal.
///   - Hora noturna (22h–05h): redução de 52min30s para cada 60min trabalhados.
/// </summary>
public sealed class CalculoHoraExtraService
{
    // Constantes de negócio (sem magic numbers)
    private const double PercentualHoraExtraNormal = 0.50;
    private const double PercentualHoraExtraFeriado = 1.00;
    private const int InicioHorarioNoturno = 22;
    private const int FimHorarioNoturno = 5;
    private static readonly TimeSpan ToleranciaAtraso = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Calcula o resultado de horas do dia para um funcionário.
    /// </summary>
    /// <param name="funcionario">Funcionário com turno contratual definido.</param>
    /// <param name="registrosDoDia">Batidas do dia em ordem cronológica.</param>
    /// <param name="ehFeriadoOuDomingo">Indica se o dia é feriado ou domingo (aplica 100%).</param>
    /// <returns><see cref="ResultadoCalculoHoraExtra"/> com o detalhamento do cálculo.</returns>
    public ResultadoCalculoHoraExtra Calcular(
        Funcionario funcionario,
        IEnumerable<RegistroPonto> registrosDoDia,
        bool ehFeriadoOuDomingo = false)
    {
        ArgumentNullException.ThrowIfNull(funcionario);
        ArgumentNullException.ThrowIfNull(registrosDoDia);

        var batidas = registrosDoDia
            .OrderBy(r => r.DataHoraBatida)
            .ToList();

        if (batidas.Count == 0)
            return ResultadoCalculoHoraExtra.SemBatidas(funcionario.Id);

        var jornadaEfetiva = CalcularJornadaEfetiva(batidas);
        var cargaContratual = funcionario.TurnoContratual.CargaHorariaDiaria;

        var horasExtras = jornadaEfetiva > (cargaContratual + ToleranciaAtraso)
            ? jornadaEfetiva - cargaContratual
            : TimeSpan.Zero;

        var horasNoturnas = CalcularHorasNoturnas(batidas);

        var percentual = ehFeriadoOuDomingo
            ? PercentualHoraExtraFeriado
            : PercentualHoraExtraNormal;

        return new ResultadoCalculoHoraExtra(
            FuncionarioId: funcionario.Id,
            JornadaEfetiva: jornadaEfetiva,
            CargaContratual: cargaContratual,
            HorasExtras: horasExtras,
            HorasNoturnas: horasNoturnas,
            PercentualAdicional: percentual,
            EhFeriadoOuDomingo: ehFeriadoOuDomingo
        );
    }

    // --- Métodos privados auxiliares ---

    /// <summary>
    /// Calcula a jornada efetiva bruta subtraindo o intervalo de almoço.
    /// Considera par Entrada→Saída e SaidaAlmoco→RetornoAlmoco.
    /// </summary>
    private static TimeSpan CalcularJornadaEfetiva(List<RegistroPonto> batidas)
    {
        var entrada = batidas
            .FirstOrDefault(b => b.TipoBatida == Enums.TipoBatida.Entrada)
            ?.DataHoraBatida;

        var saida = batidas
            .LastOrDefault(b => b.TipoBatida == Enums.TipoBatida.Saida)
            ?.DataHoraBatida;

        if (entrada is null || saida is null)
            return TimeSpan.Zero;

        var jornadaBruta = saida.Value - entrada.Value;

        var intervalo = CalcularIntervaloAlmoco(batidas);

        return jornadaBruta - intervalo;
    }

    /// <summary>
    /// Calcula a duração real do intervalo de almoço registrado.
    /// </summary>
    private static TimeSpan CalcularIntervaloAlmoco(List<RegistroPonto> batidas)
    {
        var saidaAlmoco = batidas
            .FirstOrDefault(b => b.TipoBatida == Enums.TipoBatida.SaidaAlmoco)
            ?.DataHoraBatida;

        var retornoAlmoco = batidas
            .FirstOrDefault(b => b.TipoBatida == Enums.TipoBatida.RetornoAlmoco)
            ?.DataHoraBatida;

        if (saidaAlmoco is null || retornoAlmoco is null)
            return TimeSpan.Zero;

        var intervalo = new IntervaloTempo(saidaAlmoco.Value, retornoAlmoco.Value);
        return intervalo.Duracao;
    }

    /// <summary>
    /// Calcula o total de horas trabalhadas no período noturno (22h às 05h).
    /// </summary>
    private static TimeSpan CalcularHorasNoturnas(List<RegistroPonto> batidas)
    {
        var entrada = batidas
            .FirstOrDefault(b => b.TipoBatida == Enums.TipoBatida.Entrada)
            ?.DataHoraBatida;

        var saida = batidas
            .LastOrDefault(b => b.TipoBatida == Enums.TipoBatida.Saida)
            ?.DataHoraBatida;

        if (entrada is null || saida is null)
            return TimeSpan.Zero;

        TimeSpan totalNoturno = TimeSpan.Zero;
        var cursor = entrada.Value;

        while (cursor < saida.Value)
        {
            var hora = cursor.Hour;
            var ehNoturno = hora >= InicioHorarioNoturno || hora < FimHorarioNoturno;

            if (ehNoturno)
                totalNoturno = totalNoturno.Add(TimeSpan.FromMinutes(1));

            cursor = cursor.AddMinutes(1);
        }

        return totalNoturno;
    }
}

/// <summary>
/// Value Object de saída do <see cref="CalculoHoraExtraService"/>.
/// </summary>
public sealed record ResultadoCalculoHoraExtra(
    Guid FuncionarioId,
    TimeSpan JornadaEfetiva,
    TimeSpan CargaContratual,
    TimeSpan HorasExtras,
    TimeSpan HorasNoturnas,
    double PercentualAdicional,
    bool EhFeriadoOuDomingo)
{
    /// <summary>Indica se houve horas extras no dia.</summary>
    public bool PossuiHorasExtras => HorasExtras > TimeSpan.Zero;

    /// <summary>
    /// Valor monetário das horas extras (em horas decimais × percentual).
    /// A multiplicação pelo salário-hora é responsabilidade da camada Application.
    /// </summary>
    public double HorasExtrasDecimal =>
        HorasExtras.TotalHours * (1 + PercentualAdicional);

    /// <summary>Constrói um resultado vazio para o caso de ausência de batidas.</summary>
    public static ResultadoCalculoHoraExtra SemBatidas(Guid funcionarioId) =>
        new(
            FuncionarioId: funcionarioId,
            JornadaEfetiva: TimeSpan.Zero,
            CargaContratual: TimeSpan.Zero,
            HorasExtras: TimeSpan.Zero,
            HorasNoturnas: TimeSpan.Zero,
            PercentualAdicional: 0,
            EhFeriadoOuDomingo: false
        );
}
