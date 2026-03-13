using Application.UseCases;
using Domain.Entities;

namespace Domain.Tests.UseCases;

/// <summary>
/// Testes unitários para GerarHoleriteNarrativoUseCase.
/// Foca na orquestração: busca de dados, chamada ao AuditorIA e montagem da narrativa.
/// </summary>
public sealed class GerarHoleriteNarrativoUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.Parse("22222222-0000-0000-0000-000000000001");
    private static readonly Guid FuncionarioId = Guid.Parse("22222222-0000-0000-0000-000000000002");
    private static readonly DateOnly PeriodoInicio = new(2026, 3, 1);
    private static readonly DateOnly PeriodoFim = new(2026, 3, 31);

    private readonly Mock<IFuncionarioRepository> _funcionarioRepoMock = new();
    private readonly Mock<IRegistroPontoRepository> _pontoRepoMock = new();
    private readonly Mock<IAuditorIaService> _auditorMock = new();

    private GerarHoleriteNarrativoUseCase CriarUseCase() =>
        new(_funcionarioRepoMock.Object, _pontoRepoMock.Object, _auditorMock.Object);

    private static Funcionario CriarFuncionarioValido() =>
        Funcionario.Criar(
            EmpresaId, "Carlos Mendes",
            new Cpf("529.982.247-25"),
            "F200",
            new DateTime(2025, 6, 1),
            new TurnoTrabalho(
                new TimeOnly(8, 0),
                new TimeOnly(17, 0),
                TimeSpan.FromHours(1)));

    [Fact]
    public async Task Executar_SemAnomalias_DeveRetornarNarrativaPositiva()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();

        _funcionarioRepoMock
            .Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(funcionario);

        _pontoRepoMock
            .Setup(r => r.ListarPorPeriodoAsync(
                FuncionarioId, EmpresaId, PeriodoInicio, PeriodoFim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<RegistroPonto>());

        _auditorMock
            .Setup(a => a.AnalisarAsync(
                funcionario, It.IsAny<IEnumerable<RegistroPonto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<AlertaAnomalia>());

        // Act
        var useCase = CriarUseCase();
        var resultado = await useCase.ExecutarAsync(EmpresaId, FuncionarioId, PeriodoInicio, PeriodoFim);

        // Assert
        resultado.Should().NotBeNull();
        resultado.TotalAnomalias.Should().Be(0);
        resultado.TotalHorasExtras.Should().Be(0m);
        resultado.TextoNarrativo.Should().Contain("não apresentou anomalias");
        resultado.NomeFuncionario.Should().Be("Carlos Mendes");
        resultado.EmpresaId.Should().Be(EmpresaId);
    }

    [Fact]
    public async Task Executar_ComAnomalias_DeveRetornarNarrativaComAlerta()
    {
        // Arrange
        var funcionario = CriarFuncionarioValido();

        var alertas = new[]
        {
            AlertaAnomalia.Criar(EmpresaId, FuncionarioId,
                TipoAnomalia.HoraExtraInesperada,
                new DateOnly(2026, 3, 5),
                "Hora extra detectada", 2),
            AlertaAnomalia.Criar(EmpresaId, FuncionarioId,
                TipoAnomalia.IntervaloInsuficiente,
                new DateOnly(2026, 3, 6),
                "Intervalo abaixo do mínimo", 3)
        };

        _funcionarioRepoMock
            .Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(funcionario);

        _pontoRepoMock
            .Setup(r => r.ListarPorPeriodoAsync(
                FuncionarioId, EmpresaId, PeriodoInicio, PeriodoFim, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<RegistroPonto>());

        _auditorMock
            .Setup(a => a.AnalisarAsync(
                funcionario, It.IsAny<IEnumerable<RegistroPonto>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(alertas);

        // Act
        var useCase = CriarUseCase();
        var resultado = await useCase.ExecutarAsync(EmpresaId, FuncionarioId, PeriodoInicio, PeriodoFim);

        // Assert
        resultado.TotalAnomalias.Should().Be(2);
        resultado.TotalHorasExtras.Should().Be(0.5m); // 1 HoraExtraInesperada × 0.5
        resultado.TextoNarrativo.Should().Contain("2 ocorrência(s)");
    }

    [Fact]
    public async Task Executar_FuncionarioNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _funcionarioRepoMock
            .Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Funcionario?)null);

        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(
            EmpresaId, FuncionarioId, PeriodoInicio, PeriodoFim);

        await acao.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{FuncionarioId}*");
    }

    [Fact]
    public async Task Executar_PeriodoInvalido_DeveLancarArgumentException()
    {
        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(
            EmpresaId, FuncionarioId,
            dataInicio: new DateOnly(2026, 3, 31),
            dataFim: new DateOnly(2026, 3, 1));

        await acao.Should().ThrowAsync<ArgumentException>();
    }
}
