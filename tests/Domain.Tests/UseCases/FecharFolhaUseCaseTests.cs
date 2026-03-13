using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class FecharFolhaUseCaseTests
{
    private readonly Mock<IFechamentoFolhaRepository> _fechamentoRepo = new();
    private readonly Mock<IFuncionarioRepository> _funcionarioRepo = new();
    private readonly Mock<IRegistroPontoRepository> _registroRepo = new();
    private readonly Mock<IAlertaAnomaliaQueryRepository> _alertaRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CalculoHoraExtraService _calculoService = new();

    private FecharFolhaUseCase CriarUseCase() =>
        new(_fechamentoRepo.Object, _funcionarioRepo.Object, _registroRepo.Object,
            _alertaRepo.Object, _calculoService, _uow.Object);

    [Fact]
    public async Task ExecutarAsync_PeriodoValido_SemFuncionarios_CriaFechamento()
    {
        var empresaId = Guid.NewGuid();
        var input = new FecharFolhaInputDTO(empresaId, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        _fechamentoRepo.Setup(r => r.ObterAbertoPorPeriodoAsync(empresaId, input.PeriodoInicio, input.PeriodoFim, default))
            .ReturnsAsync((FechamentoFolha?)null);
        _alertaRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, input.PeriodoInicio, input.PeriodoFim, default))
            .ReturnsAsync(new List<AlertaAnomalia>());
        _funcionarioRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(Enumerable.Empty<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(input);

        Assert.Equal("Fechada", result.Status);
        Assert.Equal(0m, result.TotalHorasExtras);
        Assert.Equal(0m, result.TotalDescontos);
        _fechamentoRepo.Verify(r => r.AdicionarAsync(It.IsAny<FechamentoFolha>(), default), Times.Once);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_PeriodoDuplicado_LancaInvalidOperationException()
    {
        var empresaId = Guid.NewGuid();
        var input = new FecharFolhaInputDTO(empresaId, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var fechamentoExistente = FechamentoFolha.Abrir(empresaId, input.PeriodoInicio, input.PeriodoFim);
        _fechamentoRepo.Setup(r => r.ObterAbertoPorPeriodoAsync(empresaId, input.PeriodoInicio, input.PeriodoFim, default))
            .ReturnsAsync(fechamentoExistente);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CriarUseCase().ExecutarAsync(input));
    }

    [Fact]
    public async Task ExecutarAsync_PeriodoFimAnteriorInicio_LancaArgumentException()
    {
        var input = new FecharFolhaInputDTO(
            Guid.NewGuid(),
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1));

        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(input));
    }

    [Fact]
    public async Task ExecutarAsync_EmpresaIdVazio_LancaArgumentException()
    {
        var input = new FecharFolhaInputDTO(
            Guid.Empty,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(input));
    }

    [Fact]
    public async Task ExecutarAsync_ComAnomaliasCriticas_ContabilizaCorretamente()
    {
        var empresaId = Guid.NewGuid();
        var funcId = Guid.NewGuid();
        var input = new FecharFolhaInputDTO(empresaId, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        var alertas = new List<AlertaAnomalia>
        {
            AlertaAnomalia.Criar(empresaId, funcId, TipoAnomalia.HoraExtraInesperada, new DateOnly(2026, 3, 5), "desc", 3),
            AlertaAnomalia.Criar(empresaId, funcId, TipoAnomalia.FaltaDeRegistro, new DateOnly(2026, 3, 6), "desc", 2),
        };

        _fechamentoRepo.Setup(r => r.ObterAbertoPorPeriodoAsync(empresaId, input.PeriodoInicio, input.PeriodoFim, default))
            .ReturnsAsync((FechamentoFolha?)null);
        _alertaRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, input.PeriodoInicio, input.PeriodoFim, default))
            .ReturnsAsync(alertas);
        _funcionarioRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(Enumerable.Empty<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(input);

        Assert.Equal(1, result.TotalAnomaliasCriticas);
    }
}
