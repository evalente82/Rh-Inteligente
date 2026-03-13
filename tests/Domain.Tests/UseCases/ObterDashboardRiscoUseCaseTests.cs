using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class ObterDashboardRiscoUseCaseTests
{
    private readonly Mock<IAlertaAnomaliaQueryRepository> _queryRepo = new();
    private readonly Mock<IFuncionarioRepository> _funcRepo = new();

    private ObterDashboardRiscoUseCase CriarUseCase() =>
        new(_queryRepo.Object, _funcRepo.Object);

    private static AlertaAnomalia CriarAlerta(Guid empresaId, Guid funcId, int gravidade, TipoAnomalia tipo, bool resolvido = false)
    {
        var alerta = AlertaAnomalia.Criar(empresaId, funcId, tipo, DateOnly.FromDateTime(DateTime.Today), "desc", gravidade);
        if (resolvido) alerta.MarcarComoResolvido();
        return alerta;
    }

    [Fact]
    public async Task ExecutarAsync_SemAlertas_RetornaDashboardZerado()
    {
        var empresaId = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        _queryRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, inicio, fim, default))
            .ReturnsAsync(new List<AlertaAnomalia>());
        _funcRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(new List<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(empresaId, inicio, fim);

        Assert.Equal(0, result.TotalAlertas);
        Assert.Equal(0, result.TotalCriticos);
        Assert.Empty(result.AnomaliasPorTipo);
        Assert.Empty(result.TopFuncionariosRisco);
    }

    [Fact]
    public async Task ExecutarAsync_ComAlertas_AgregaCorretamente()
    {
        var empresaId = Guid.NewGuid();
        var func1 = Guid.NewGuid();
        var func2 = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        var alertas = new List<AlertaAnomalia>
        {
            CriarAlerta(empresaId, func1, 3, TipoAnomalia.HoraExtraInesperada),
            CriarAlerta(empresaId, func1, 3, TipoAnomalia.HoraExtraInesperada),
            CriarAlerta(empresaId, func2, 2, TipoAnomalia.IntervaloInsuficiente),
            CriarAlerta(empresaId, func1, 1, TipoAnomalia.FaltaDeRegistro, resolvido: true),
        };

        _queryRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, inicio, fim, default))
            .ReturnsAsync(alertas);
        _funcRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(new List<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(empresaId, inicio, fim);

        Assert.Equal(4, result.TotalAlertas);
        Assert.Equal(2, result.TotalCriticos);
        Assert.Equal(1, result.TotalAtencao);
        Assert.Equal(1, result.TotalInformativos);
        Assert.Equal(1, result.TotalResolvidos);
        Assert.Equal(3, result.AnomaliasPorTipo.Count);
    }

    [Fact]
    public async Task ExecutarAsync_PeriodoInvalido_LancaArgumentException()
    {
        var empresaId = Guid.NewGuid();
        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(
                empresaId,
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 3, 1))); // fim < inicio
    }

    [Fact]
    public async Task ExecutarAsync_EmpresaIdVazio_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(
                Guid.Empty,
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 3, 31)));
    }
}
