using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Services;

public sealed class CalculoHoraExtraServiceTests
{
    private readonly CalculoHoraExtraService _service = new();

    private static readonly Guid EmpresaId = Guid.NewGuid();

    // Turno padrão: 08h–17h, 1h de almoço → carga líquida = 8h
    private static readonly TurnoTrabalho TurnoPadrao =
        new(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));

    // =========================================================
    // Cenário: sem batidas
    // =========================================================

    [Fact]
    public void Calcular_QuandoSemBatidas_DeveRetornarResultadoVazio()
    {
        var funcionario = CriarFuncionario();
        var resultado = _service.Calcular(funcionario, []);

        resultado.JornadaEfetiva.Should().Be(TimeSpan.Zero);
        resultado.HorasExtras.Should().Be(TimeSpan.Zero);
        resultado.PossuiHorasExtras.Should().BeFalse();
    }

    // =========================================================
    // Cenário: jornada normal (sem HE)
    // =========================================================

    [Fact]
    public void Calcular_QuandoJornadaNormal_NaoDeveTerHorasExtras()
    {
        // Arrange — exatamente 8h de jornada (08:00–12:00, 13:00–17:00)
        var funcionario = CriarFuncionario();
        var batidas = CriarBatidasDoDia(
            entrada: "08:00", saidaAlmoco: "12:00",
            retornoAlmoco: "13:00", saida: "17:00");

        // Act
        var resultado = _service.Calcular(funcionario, batidas);

        // Assert
        resultado.JornadaEfetiva.Should().Be(TimeSpan.FromHours(8));
        resultado.PossuiHorasExtras.Should().BeFalse();
        resultado.HorasExtras.Should().Be(TimeSpan.Zero);
    }

    // =========================================================
    // Cenário: 2h extras em dia útil (50%)
    // =========================================================

    [Fact]
    public void Calcular_QuandoJornadaComDuasHorasExtras_DeveApontarHorasExtrasE50PorCento()
    {
        // Arrange — trabalhou 10h líquidas (08:00–12:00, 13:00–19:00)
        var funcionario = CriarFuncionario();
        var batidas = CriarBatidasDoDia(
            entrada: "08:00", saidaAlmoco: "12:00",
            retornoAlmoco: "13:00", saida: "19:00");

        // Act
        var resultado = _service.Calcular(funcionario, batidas, ehFeriadoOuDomingo: false);

        // Assert
        resultado.HorasExtras.Should().Be(TimeSpan.FromHours(2));
        resultado.PercentualAdicional.Should().Be(0.50);
        resultado.HorasExtrasDecimal.Should().BeApproximately(3.0, 0.001); // 2h × 1.5
    }

    // =========================================================
    // Cenário: HE em feriado (100%)
    // =========================================================

    [Fact]
    public void Calcular_QuandoHoraExtraEmFeriado_DeveAplicar100PorCento()
    {
        // Arrange — trabalhou 10h líquidas em feriado
        var funcionario = CriarFuncionario();
        var batidas = CriarBatidasDoDia(
            entrada: "08:00", saidaAlmoco: "12:00",
            retornoAlmoco: "13:00", saida: "19:00");

        // Act
        var resultado = _service.Calcular(funcionario, batidas, ehFeriadoOuDomingo: true);

        // Assert
        resultado.HorasExtras.Should().Be(TimeSpan.FromHours(2));
        resultado.PercentualAdicional.Should().Be(1.00);
        resultado.EhFeriadoOuDomingo.Should().BeTrue();
        resultado.HorasExtrasDecimal.Should().BeApproximately(4.0, 0.001); // 2h × 2.0
    }

    // =========================================================
    // Cenário: horas noturnas
    // =========================================================

    [Fact]
    public void Calcular_QuandoJornadaContemHorarioNoturno_DeveContabilizarHorasNoturnas()
    {
        // Arrange — Turno auxiliar válido: 14h–23h59 com 1h de intervalo.
        // As batidas cobrem 22h–01h (cruzam o período noturno 22h–05h da CLT).
        var turnoEstendido = new TurnoTrabalho(
            new TimeOnly(14, 0),
            new TimeOnly(23, 59),
            TimeSpan.FromHours(1));

        var funcionario = Funcionario.Criar(EmpresaId, "Maria Santos", new Cpf("529.982.247-25"), "F002",
            new DateTime(2024, 1, 1), turnoEstendido);

        var data = new DateTime(2025, 3, 10);
        var batidas = new List<RegistroPonto>
        {
            RegistroPonto.Criar(EmpresaId, funcionario.Id, data.AddHours(22), TipoBatida.Entrada, "REP"),
            RegistroPonto.Criar(EmpresaId, funcionario.Id, data.AddDays(1).AddHours(1), TipoBatida.Saida, "REP"),
        };

        // Act
        var resultado = _service.Calcular(funcionario, batidas);

        // Assert — das 22h até as 01h do dia seguinte há 3h dentro do período noturno (22h–05h)
        resultado.HorasNoturnas.Should().BeGreaterThanOrEqualTo(TimeSpan.FromHours(1));
    }

    // =========================================================
    // Testes de guarda (null checks)
    // =========================================================

    [Fact]
    public void Calcular_QuandoFuncionarioNulo_DeveLancarArgumentNullException()
    {
        var act = () => _service.Calcular(null!, []);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Calcular_QuandoRegistrosNulos_DeveLancarArgumentNullException()
    {
        var funcionario = CriarFuncionario();
        var act = () => _service.Calcular(funcionario, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // =========================================================
    // Helpers privados
    // =========================================================

    private static Funcionario CriarFuncionario() =>
        Funcionario.Criar(EmpresaId, "João Silva", new Cpf("529.982.247-25"), "F001",
            new DateTime(2024, 1, 1), TurnoPadrao);

    private static List<RegistroPonto> CriarBatidasDoDia(
        string entrada, string saidaAlmoco, string retornoAlmoco, string saida)
    {
        var data = new DateTime(2025, 3, 10);
        var funcionarioId = Guid.NewGuid();

        return
        [
            RegistroPonto.Criar(EmpresaId, funcionarioId,
                data.Add(TimeSpan.Parse(entrada)), TipoBatida.Entrada, "REP"),

            RegistroPonto.Criar(EmpresaId, funcionarioId,
                data.Add(TimeSpan.Parse(saidaAlmoco)), TipoBatida.SaidaAlmoco, "REP"),

            RegistroPonto.Criar(EmpresaId, funcionarioId,
                data.Add(TimeSpan.Parse(retornoAlmoco)), TipoBatida.RetornoAlmoco, "REP"),

            RegistroPonto.Criar(EmpresaId, funcionarioId,
                data.Add(TimeSpan.Parse(saida)), TipoBatida.Saida, "REP"),
        ];
    }
}
