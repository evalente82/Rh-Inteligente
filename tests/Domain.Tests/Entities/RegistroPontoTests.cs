using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Domain.Tests.Entities;

public sealed class RegistroPontoTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();

    [Fact]
    public void Criar_QuandoDadosValidos_DeveRetornarRegistroCorreto()
    {
        // Act
        var registro = RegistroPonto.Criar(
            empresaId: EmpresaId,
            funcionarioId: FuncionarioId,
            dataHoraBatida: new DateTime(2025, 3, 10, 8, 0, 0),
            tipoBatida: TipoBatida.Entrada,
            origem: "REP");

        // Assert
        registro.Id.Should().NotBeEmpty();
        registro.EmpresaId.Should().Be(EmpresaId);
        registro.FuncionarioId.Should().Be(FuncionarioId);
        registro.TipoBatida.Should().Be(TipoBatida.Entrada);
        registro.Origem.Should().Be("REP");
        registro.LancamentoManual.Should().BeFalse();
        registro.Justificativa.Should().BeNull();
    }

    [Fact]
    public void Criar_QuandoEmpresaIdVazio_DeveLancarArgumentException()
    {
        var act = () => RegistroPonto.Criar(Guid.Empty, FuncionarioId,
            DateTime.UtcNow, TipoBatida.Entrada, "REP");

        act.Should().Throw<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Fact]
    public void Criar_QuandoFuncionarioIdVazio_DeveLancarArgumentException()
    {
        var act = () => RegistroPonto.Criar(EmpresaId, Guid.Empty,
            DateTime.UtcNow, TipoBatida.Entrada, "REP");

        act.Should().Throw<ArgumentException>().WithMessage("*FuncionarioId*");
    }

    [Fact]
    public void Criar_QuandoLancamentoManualSemJustificativa_DeveLancarInvalidOperationException()
    {
        var act = () => RegistroPonto.Criar(
            empresaId: EmpresaId,
            funcionarioId: FuncionarioId,
            dataHoraBatida: DateTime.UtcNow,
            tipoBatida: TipoBatida.Entrada,
            origem: "Manual",
            lancamentoManual: true,
            justificativa: null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*justificativa*");
    }

    [Fact]
    public void Criar_QuandoLancamentoManualComJustificativa_DeveCriarRegistro()
    {
        // Act
        var registro = RegistroPonto.Criar(
            empresaId: EmpresaId,
            funcionarioId: FuncionarioId,
            dataHoraBatida: DateTime.UtcNow,
            tipoBatida: TipoBatida.Entrada,
            origem: "Manual",
            lancamentoManual: true,
            justificativa: "Falha no REP no dia 10/03");

        // Assert
        registro.LancamentoManual.Should().BeTrue();
        registro.Justificativa.Should().Be("Falha no REP no dia 10/03");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_QuandoOrigemVaziaOuEspacos_DeveLancarArgumentException(string origem)
    {
        var act = () => RegistroPonto.Criar(EmpresaId, FuncionarioId,
            DateTime.UtcNow, TipoBatida.Entrada, origem);

        act.Should().Throw<ArgumentException>();
    }
}
