using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.ValueObjects;

public sealed class IntervaloTempoTests
{
    [Fact]
    public void Criar_QuandoDadosValidos_DeveCalcularDuracao()
    {
        // Arrange
        var inicio = new DateTime(2025, 3, 10, 12, 0, 0);
        var fim = new DateTime(2025, 3, 10, 13, 0, 0);

        // Act
        var intervalo = new IntervaloTempo(inicio, fim);

        // Assert
        intervalo.Duracao.Should().Be(TimeSpan.FromHours(1));
    }

    [Fact]
    public void Criar_QuandoFimAnteriorAoInicio_DeveLancarArgumentException()
    {
        var act = () => new IntervaloTempo(
            new DateTime(2025, 3, 10, 13, 0, 0),
            new DateTime(2025, 3, 10, 12, 0, 0));

        act.Should().Throw<ArgumentException>().WithMessage("*posterior*início*");
    }

    [Fact]
    public void RespeitaMinimo_QuandoIntervaloSuficiente_DeveRetornarTrue()
    {
        var intervalo = new IntervaloTempo(
            new DateTime(2025, 3, 10, 12, 0, 0),
            new DateTime(2025, 3, 10, 13, 5, 0));

        intervalo.RespeitaMinimo(IntervaloTempo.MinimoLegalClt).Should().BeTrue();
    }

    [Fact]
    public void RespeitaMinimo_QuandoIntervaloInsuficiente_DeveRetornarFalse()
    {
        var intervalo = new IntervaloTempo(
            new DateTime(2025, 3, 10, 12, 0, 0),
            new DateTime(2025, 3, 10, 12, 45, 0));

        intervalo.RespeitaMinimo(IntervaloTempo.MinimoLegalClt).Should().BeFalse();
    }

    [Fact]
    public void DoisIntervalosComMesmosDados_DevemSerIguaisPorIgualdadeEstrutural()
    {
        var inicio = new DateTime(2025, 3, 10, 12, 0, 0);
        var fim = new DateTime(2025, 3, 10, 13, 0, 0);

        var i1 = new IntervaloTempo(inicio, fim);
        var i2 = new IntervaloTempo(inicio, fim);

        i1.Should().Be(i2);
    }
}
