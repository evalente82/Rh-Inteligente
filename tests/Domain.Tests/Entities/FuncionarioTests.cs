using Domain.Entities;
using Domain.ValueObjects;
using FluentAssertions;

namespace Domain.Tests.Entities;

public sealed class FuncionarioTests
{
    // --- Dados auxiliares ---
    private static readonly Guid EmpresaIdValido = Guid.NewGuid();
    private static readonly TurnoTrabalho TurnoPadrao =
        new(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));

    // =========================================================
    // Testes do factory method Criar()
    // =========================================================

    [Fact]
    public void Criar_QuandoDadosValidos_DeveRetornarFuncionarioCorreto()
    {
        // Act
        var funcionario = Funcionario.Criar(
            empresaId: EmpresaIdValido,
            nome: "João Silva",
            cpf: "123.456.789-00",
            matricula: "F001",
            dataAdmissao: new DateTime(2024, 1, 15),
            turnoContratual: TurnoPadrao);

        // Assert
        funcionario.Id.Should().NotBeEmpty();
        funcionario.EmpresaId.Should().Be(EmpresaIdValido);
        funcionario.Nome.Should().Be("João Silva");
        funcionario.Cpf.Should().Be("123.456.789-00");
        funcionario.Matricula.Should().Be("F001");
        funcionario.Ativo.Should().BeTrue();
        funcionario.DataDemissao.Should().BeNull();
        funcionario.TurnoContratual.Should().Be(TurnoPadrao);
    }

    [Fact]
    public void Criar_QuandoEmpresaIdVazio_DeveLancarArgumentException()
    {
        // Act
        var act = () => Funcionario.Criar(
            empresaId: Guid.Empty,
            nome: "João Silva",
            cpf: "123.456.789-00",
            matricula: "F001",
            dataAdmissao: DateTime.UtcNow,
            turnoContratual: TurnoPadrao);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*EmpresaId*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_QuandoNomeVazioOuEspacos_DeveLancarArgumentException(string nome)
    {
        var act = () => Funcionario.Criar(EmpresaIdValido, nome, "123", "F001", DateTime.UtcNow, TurnoPadrao);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_QuandoMatriculaVaziaOuEspacos_DeveLancarArgumentException(string matricula)
    {
        var act = () => Funcionario.Criar(EmpresaIdValido, "Nome", "123", matricula, DateTime.UtcNow, TurnoPadrao);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Criar_QuandoTurnoNulo_DeveLancarArgumentNullException()
    {
        var act = () => Funcionario.Criar(EmpresaIdValido, "João", "123", "F001", DateTime.UtcNow, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // =========================================================
    // Testes do método Demitir()
    // =========================================================

    [Fact]
    public void Demitir_QuandoDataValida_DeveDefinirDataDemissaoEInativar()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        // Data no passado para garantir Ativo = false imediatamente
        var dataDemissao = new DateTime(2024, 6, 1);

        // Act
        funcionario.Demitir(dataDemissao);

        // Assert
        funcionario.DataDemissao.Should().Be(dataDemissao);
        funcionario.Ativo.Should().BeFalse();
    }

    [Fact]
    public void Demitir_QuandoDataAnteriorAdmissao_DeveLancarInvalidOperationException()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        var dataAnterior = new DateTime(2020, 1, 1); // anterior à admissão

        // Act
        var act = () => funcionario.Demitir(dataAnterior);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*demissão*anterior*admissão*");
    }

    // =========================================================
    // Testes do método AtualizarTurno()
    // =========================================================

    [Fact]
    public void AtualizarTurno_QuandoTurnoValido_DeveAtualizarTurnoContratual()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        var novoTurno = new TurnoTrabalho(new TimeOnly(9, 0), new TimeOnly(18, 0), TimeSpan.FromHours(1));

        // Act
        funcionario.AtualizarTurno(novoTurno);

        // Assert
        funcionario.TurnoContratual.Should().Be(novoTurno);
    }

    [Fact]
    public void AtualizarTurno_QuandoTurnoNulo_DeveLancarArgumentNullException()
    {
        var funcionario = CriarFuncionarioValido();
        var act = () => funcionario.AtualizarTurno(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // --- Helper ---
    private static Funcionario CriarFuncionarioValido() =>
        Funcionario.Criar(EmpresaIdValido, "João Silva", "12345678900", "F001",
            new DateTime(2024, 3, 1), TurnoPadrao);
}
