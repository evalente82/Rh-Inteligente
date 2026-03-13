using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests.Entities;

public sealed class AlertaAnomaliaTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();
    private static readonly DateOnly DataReferencia = new(2025, 3, 10);

    [Fact]
    public void Criar_QuandoDadosValidos_DeveRetornarAlertaCorreto()
    {
        // Act
        var alerta = AlertaAnomalia.Criar(
            empresaId: EmpresaId,
            funcionarioId: FuncionarioId,
            tipoAnomalia: TipoAnomalia.IntervaloInsuficiente,
            dataReferencia: DataReferencia,
            descricao: "Intervalo de almoço registrado foi de 45 minutos (mínimo: 60 min CLT).",
            gravidade: 2);

        // Assert
        alerta.Id.Should().NotBeEmpty();
        alerta.EmpresaId.Should().Be(EmpresaId);
        alerta.FuncionarioId.Should().Be(FuncionarioId);
        alerta.TipoAnomalia.Should().Be(TipoAnomalia.IntervaloInsuficiente);
        alerta.Gravidade.Should().Be(2);
        alerta.Resolvido.Should().BeFalse();
        alerta.ResolvidoEm.Should().BeNull();
        alerta.GeradoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public void Criar_QuandoGravidadeForaDaFaixa_DeveLancarArgumentOutOfRangeException(int gravidade)
    {
        var act = () => AlertaAnomalia.Criar(EmpresaId, FuncionarioId,
            TipoAnomalia.HoraExtraInesperada, DataReferencia, "desc", gravidade);

        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*Gravidade*");
    }

    [Fact]
    public void Criar_QuandoEmpresaIdVazio_DeveLancarArgumentException()
    {
        var act = () => AlertaAnomalia.Criar(Guid.Empty, FuncionarioId,
            TipoAnomalia.FaltaDeRegistro, DataReferencia, "desc", 1);

        act.Should().Throw<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Fact]
    public void MarcarComoResolvido_QuandoNaoResolvido_DeveDefinirResolvidoEResolvidoEm()
    {
        // Arrange
        var alerta = CriarAlertaValido();

        // Act
        alerta.MarcarComoResolvido();

        // Assert
        alerta.Resolvido.Should().BeTrue();
        alerta.ResolvidoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarcarComoResolvido_QuandoJaResolvido_DeveLancarInvalidOperationException()
    {
        // Arrange
        var alerta = CriarAlertaValido();
        alerta.MarcarComoResolvido();

        // Act
        var act = () => alerta.MarcarComoResolvido();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*já foi resolvido*");
    }

    // --- Helper ---
    private static AlertaAnomalia CriarAlertaValido() =>
        AlertaAnomalia.Criar(EmpresaId, FuncionarioId,
            TipoAnomalia.JornadaExcedida, DataReferencia,
            "Jornada diária excedeu 10 horas.", 3);
}
