using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.UseCases;

public sealed class AnalisarRegistrosComIaUseCaseTests
{
    private readonly Mock<IFuncionarioRepository> _funcionarioRepoMock = new();
    private readonly Mock<IRegistroPontoRepository> _registroRepoMock = new();
    private readonly Mock<IAuditorIaService> _auditorIaMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    private AnalisarRegistrosComIaUseCase CriarUseCase() =>
        new(_funcionarioRepoMock.Object, _registroRepoMock.Object,
            _auditorIaMock.Object, _unitOfWorkMock.Object);

    // Dados auxiliares
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();
    private static readonly DateOnly PeriodoInicio = new(2026, 3, 1);
    private static readonly DateOnly PeriodoFim = new(2026, 3, 31);

    private static readonly TurnoTrabalho TurnoPadrao =
        new(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));

    private static Funcionario CriarFuncionarioValido() =>
        Funcionario.Criar(EmpresaId, "Maria Oliveira", "98765432100",
            "F010", new DateTime(2024, 1, 1), TurnoPadrao);

    // =========================================================
    // Cenário: fluxo feliz — retorna alertas gerados pela IA
    // =========================================================

    [Fact]
    public async Task ExecutarAsync_QuandoIaRetornaAlertas_DeveMapearParaDTO()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();

        var alerta = AlertaAnomalia.Criar(EmpresaId, funcionario.Id,
            TipoAnomalia.IntervaloInsuficiente, new DateOnly(2026, 3, 10),
            "Intervalo de almoço de 40 minutos. Mínimo CLT: 60 minutos.", gravidade: 2);

        ConfigurarMocksBasicos(funcionario, registros: [], alertas: [alerta]);

        var input = new AnalisarRegistrosInputDTO(EmpresaId, funcionario.Id,
            PeriodoInicio, PeriodoFim);

        // Act
        var resultado = (await CriarUseCase().ExecutarAsync(input)).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado[0].TipoAnomalia.Should().Be(TipoAnomalia.IntervaloInsuficiente);
        resultado[0].Gravidade.Should().Be(2);
        resultado[0].FuncionarioId.Should().Be(funcionario.Id);
        resultado[0].Resolvido.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoIaNaoEncontraAnomalias_DeveRetornarListaVazia()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        ConfigurarMocksBasicos(funcionario, registros: [], alertas: []);

        var input = new AnalisarRegistrosInputDTO(EmpresaId, funcionario.Id,
            PeriodoInicio, PeriodoFim);

        // Act
        var resultado = await CriarUseCase().ExecutarAsync(input);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoFluxoCompleto_DeveCommitarUmaVez()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        ConfigurarMocksBasicos(funcionario, registros: [], alertas: []);

        var input = new AnalisarRegistrosInputDTO(EmpresaId, funcionario.Id,
            PeriodoInicio, PeriodoFim);

        // Act
        await CriarUseCase().ExecutarAsync(input);

        // Assert — o commit deve ser chamado exatamente uma vez
        _unitOfWorkMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_QuandoFluxoCompleto_DeveChamarAuditorIaUmaVez()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();
        ConfigurarMocksBasicos(funcionario, registros: [], alertas: []);

        var input = new AnalisarRegistrosInputDTO(EmpresaId, funcionario.Id,
            PeriodoInicio, PeriodoFim);

        // Act
        await CriarUseCase().ExecutarAsync(input);

        // Assert
        _auditorIaMock.Verify(a => a.AnalisarAsync(
            It.Is<Funcionario>(f => f.Id == funcionario.Id),
            It.IsAny<IEnumerable<RegistroPonto>>(),
            default), Times.Once);
    }

    // =========================================================
    // Cenário: funcionário não encontrado
    // =========================================================

    [Fact]
    public async Task ExecutarAsync_QuandoFuncionarioNaoEncontrado_DeveLancarInvalidOperationException()
    {
        // Arrange — repositório retorna null
        _funcionarioRepoMock
            .Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((Funcionario?)null);

        var input = new AnalisarRegistrosInputDTO(EmpresaId, FuncionarioId,
            PeriodoInicio, PeriodoFim);

        // Act
        var act = async () => await CriarUseCase().ExecutarAsync(input);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não encontrado*");
    }

    // =========================================================
    // Cenários: validação de entrada
    // =========================================================

    [Fact]
    public async Task ExecutarAsync_QuandoInputNulo_DeveLancarArgumentNullException()
    {
        var act = async () => await CriarUseCase().ExecutarAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoEmpresaIdVazio_DeveLancarArgumentException()
    {
        var input = new AnalisarRegistrosInputDTO(Guid.Empty, FuncionarioId,
            PeriodoInicio, PeriodoFim);

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Fact]
    public async Task ExecutarAsync_QuandoFuncionarioIdVazio_DeveLancarArgumentException()
    {
        var input = new AnalisarRegistrosInputDTO(EmpresaId, Guid.Empty,
            PeriodoInicio, PeriodoFim);

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*FuncionarioId*");
    }

    [Fact]
    public async Task ExecutarAsync_QuandoPeriodoFimAnteriorAoInicio_DeveLancarArgumentException()
    {
        // PeriodoInicio = 31/03, PeriodoFim = 01/03 → invertido
        var input = new AnalisarRegistrosInputDTO(EmpresaId, FuncionarioId,
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1));

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*período*");
    }

    // =========================================================
    // Helper
    // =========================================================

    private void ConfigurarMocksBasicos(
        Funcionario funcionario,
        IEnumerable<RegistroPonto> registros,
        IEnumerable<AlertaAnomalia> alertas)
    {
        _funcionarioRepoMock
            .Setup(r => r.ObterPorIdAsync(funcionario.Id, EmpresaId, default))
            .ReturnsAsync(funcionario);

        _registroRepoMock
            .Setup(r => r.ListarPorPeriodoAsync(funcionario.Id, EmpresaId,
                PeriodoInicio, PeriodoFim, default))
            .ReturnsAsync(registros);

        _auditorIaMock
            .Setup(a => a.AnalisarAsync(It.IsAny<Funcionario>(),
                It.IsAny<IEnumerable<RegistroPonto>>(), default))
            .ReturnsAsync(alertas);

        _unitOfWorkMock
            .Setup(u => u.CommitAsync(default))
            .ReturnsAsync(1);
    }
}
