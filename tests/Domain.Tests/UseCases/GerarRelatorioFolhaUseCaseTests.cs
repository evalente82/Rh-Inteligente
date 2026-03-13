using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class GerarRelatorioFolhaUseCaseTests
{
    private readonly Mock<IFechamentoFolhaRepository> _fechamentoRepo = new();
    private readonly Mock<IAlertaAnomaliaQueryRepository> _alertaRepo = new();
    private readonly Mock<IEmbeddingService> _embeddingService = new();
    private readonly Mock<IVectorRepository> _vectorRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private GerarRelatorioFolhaUseCase CriarUseCase() =>
        new(_fechamentoRepo.Object, _alertaRepo.Object,
            _embeddingService.Object, _vectorRepo.Object, _uow.Object);

    private FechamentoFolha CriarFolhaFechada(Guid empresaId)
    {
        var folha = FechamentoFolha.Abrir(empresaId, new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        folha.Fechar(8m, 0.5m, 1, "Narrativa inicial.");
        return folha;
    }

    [Fact]
    public async Task ExecutarAsync_FechamentoEncontrado_GeraRelatorio()
    {
        var empresaId = Guid.NewGuid();
        var folha = CriarFolhaFechada(empresaId);

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(folha.Id, default))
            .ReturnsAsync(folha);
        _alertaRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, folha.PeriodoInicio, folha.PeriodoFim, default))
            .ReturnsAsync(new List<AlertaAnomalia>());
        _embeddingService.Setup(e => e.GerarEmbeddingAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new float[768]);
        _vectorRepo.Setup(v => v.BuscarSimilaresAsync(empresaId, It.IsAny<float[]>(), It.IsAny<int>(), default))
            .ReturnsAsync(Enumerable.Empty<ChunkVetorizado>());

        var result = await CriarUseCase().ExecutarAsync(folha.Id);

        Assert.NotNull(result.RelatorioNarrativo);
        Assert.Contains("RELATÓRIO DE FECHAMENTO", result.RelatorioNarrativo);
        _uow.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_FechamentoNaoEncontrado_LancaKeyNotFoundException()
    {
        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((FechamentoFolha?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CriarUseCase().ExecutarAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ExecutarAsync_FechamentoIdVazio_LancaArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarUseCase().ExecutarAsync(Guid.Empty));
    }

    [Fact]
    public async Task ExecutarAsync_ComChunksCct_IncluiContextoNarrativa()
    {
        var empresaId = Guid.NewGuid();
        var folha = CriarFolhaFechada(empresaId);
        var chunk = new ChunkVetorizado(
            Guid.NewGuid(), empresaId,
            "Art. 59 CLT — horas extras com adicional de 50%.",
            new float[768], "cct_2026.pdf", 5);

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(folha.Id, default))
            .ReturnsAsync(folha);
        _alertaRepo.Setup(r => r.ListarPorPeriodoAsync(empresaId, folha.PeriodoInicio, folha.PeriodoFim, default))
            .ReturnsAsync(new List<AlertaAnomalia>());
        _embeddingService.Setup(e => e.GerarEmbeddingAsync(It.IsAny<string>(), default))
            .ReturnsAsync(new float[768]);
        _vectorRepo.Setup(v => v.BuscarSimilaresAsync(empresaId, It.IsAny<float[]>(), It.IsAny<int>(), default))
            .ReturnsAsync(new[] { chunk });

        var result = await CriarUseCase().ExecutarAsync(folha.Id);

        Assert.Contains("Art. 59 CLT", result.RelatorioNarrativo);
    }
}
