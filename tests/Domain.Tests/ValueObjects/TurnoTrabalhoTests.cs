using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public sealed class TurnoTrabalhoTests
{
    [Fact]
    public void Criar_QuandoDadosValidos_DeveCalcularCargaHoraria()
    {
        // Arrange
        var entrada = new TimeOnly(8, 0);
        var saida = new TimeOnly(17, 0);
        var intervalo = TimeSpan.FromHours(1);

        // Act
        var turno = new TurnoTrabalho(entrada, saida, intervalo);

        // Assert
        turno.CargaHorariaDiaria.Should().Be(TimeSpan.FromHours(8));
    }

    [Fact]
    public void Criar_QuandoSaidaMenorQueEntrada_DeveLancarArgumentException()
    {
        var act = () => new TurnoTrabalho(
            new TimeOnly(17, 0),
            new TimeOnly(8, 0),
            TimeSpan.FromHours(1));

        act.Should().Throw<ArgumentException>().WithMessage("*saída*entrada*");
    }

    [Fact]
    public void Criar_QuandoIntervaloNegativo_DeveLancarArgumentException()
    {
        var act = () => new TurnoTrabalho(
            new TimeOnly(8, 0),
            new TimeOnly(17, 0),
            TimeSpan.FromMinutes(-30));

        act.Should().Throw<ArgumentException>().WithMessage("*intervalo*negativa*");
    }

    [Fact]
    public void DoisTurnosComMesmosDados_DevemSerIguaisPorIgualdadeEstrutural()
    {
        var turno1 = new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));
        var turno2 = new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));

        turno1.Should().Be(turno2);
    }
}
