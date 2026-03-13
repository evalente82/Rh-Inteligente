using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class ObterIndiceConformidadeUseCaseTests
{
    private readonly Mock<IAlertaAnomaliaQueryRepository> _queryRepo = new();
    private readonly Mock<IFuncionarioRepository> _funcRepo = new();

    private ObterIndiceConformidadeUseCase CriarUseCase() =>
        new(_queryRepo.Object, _funcRepo.Object);

    private static AlertaAnomalia CriarAlerta(Guid empresaId, Guid funcId, int gravidade, bool resolvido = false)
    {
        var alerta = AlertaAnomalia.Criar(empresaId, funcId, TipoAnomalia.HoraExtraInesperada,
            DateOnly.FromDateTime(DateTime.Today), "desc", gravidade);
        if (resolvido) alerta.MarcarComoResolvido();
        return alerta;
    }

    [Fact]
    public async Task ExecutarAsync_SemAlertas_RetornaIndice100Verde()
    {
        var empresaId = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        _queryRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, inicio, fim, default))
            .ReturnsAsync(new List<AlertaAnomalia>());
        _funcRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(Enumerable.Empty<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(empresaId, inicio, fim);

        Assert.Equal(100.0, result.IndiceConformidade);
        Assert.Equal("Verde", result.Classificacao);
    }

    [Fact]
    public async Task ExecutarAsync_TodosCriticos_RetornaIndice0Vermelho()
    {
        var empresaId = Guid.NewGuid();
        var funcId = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        var alertas = new List<AlertaAnomalia>
        {
            CriarAlerta(empresaId, funcId, 3),
            CriarAlerta(empresaId, funcId, 3),
        };

        _queryRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, inicio, fim, default))
            .ReturnsAsync(alertas);
        _funcRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(Enumerable.Empty<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(empresaId, inicio, fim);

        Assert.Equal(0.0, result.IndiceConformidade);
        Assert.Equal("Vermelho", result.Classificacao);
    }

    [Fact]
    public async Task ExecutarAsync_AlertasResolvidos_NaoContamNoIndice()
    {
        var empresaId = Guid.NewGuid();
        var funcId = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        // 2 críticos já resolvidos — índice deve ser 100
        var alertas = new List<AlertaAnomalia>
        {
            CriarAlerta(empresaId, funcId, 3, resolvido: true),
            CriarAlerta(empresaId, funcId, 3, resolvido: true),
        };

        _queryRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, inicio, fim, default))
            .ReturnsAsync(alertas);
        _funcRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, default))
            .ReturnsAsync(Enumerable.Empty<Funcionario>());

        var result = await CriarUseCase().ExecutarAsync(empresaId, inicio, fim);

        Assert.Equal(100.0, result.IndiceConformidade);
    }

    [Fact]
    public async Task ExecutarAsync_PeriodoInvalido_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(
                Guid.NewGuid(),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 3, 1)));
    }
}
