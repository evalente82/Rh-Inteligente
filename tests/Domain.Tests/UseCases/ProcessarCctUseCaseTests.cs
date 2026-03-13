using Application.Interfaces;
using Application.UseCases;

namespace Domain.Tests.UseCases;

/// <summary>
/// Testes unitários para ProcessarCctUseCase.
/// Valida o pipeline: PDF → chunks → embeddings → Qdrant, sem chamar APIs externas.
/// </summary>
public sealed class ProcessarCctUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.Parse("11111111-0000-0000-0000-000000000001");

    private readonly Mock<ICctPdfParser> _parserMock = new();
    private readonly Mock<IEmbeddingService> _embeddingMock = new();
    private readonly Mock<IVectorRepository> _vectorMock = new();

    private ProcessarCctUseCase CriarUseCase() =>
        new(_parserMock.Object, _embeddingMock.Object, _vectorMock.Object);

    [Fact]
    public async Task Executar_PdfValido_DeveGarantirColecaoExtrairVetorizarEPersistir()
    {
        // Arrange
        var conteudo = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // magic bytes PDF
        var input = new UploadCctInputDTO(EmpresaId, "cct-2026.pdf", conteudo);

        var chunks = new[]
        {
            new ChunkTexto("Cláusula 1: Jornada de 44h semanais.", "cct-2026.pdf", 1),
            new ChunkTexto("Cláusula 2: Intervalo mínimo de 1 hora.", "cct-2026.pdf", 1)
        };

        var vetores = new[] { new float[768], new float[768] };

        _parserMock.Setup(p => p.Extrair(conteudo, "cct-2026.pdf", It.IsAny<int>()))
                   .Returns(chunks);

        _embeddingMock.Setup(e => e.GerarEmbeddingLoteAsync(
                It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vetores);

        _vectorMock.Setup(v => v.GarantirColecaoAsync(EmpresaId, 768, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        _vectorMock.Setup(v => v.SalvarChunksAsync(
                EmpresaId, It.IsAny<IEnumerable<ChunkVetorizado>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var useCase = CriarUseCase();
        await useCase.ExecutarAsync(input);

        // Assert
        _vectorMock.Verify(v =>
            v.GarantirColecaoAsync(EmpresaId, 768, It.IsAny<CancellationToken>()), Times.Once);

        _parserMock.Verify(p =>
            p.Extrair(conteudo, "cct-2026.pdf", It.IsAny<int>()), Times.Once);

        _embeddingMock.Verify(e =>
            e.GerarEmbeddingLoteAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _vectorMock.Verify(v =>
            v.SalvarChunksAsync(EmpresaId, It.Is<IEnumerable<ChunkVetorizado>>(
                col => col.Count() == 2), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Executar_PdfVazio_DeveLancarArgumentException()
    {
        var input = new UploadCctInputDTO(EmpresaId, "vazio.pdf", Array.Empty<byte>());
        var useCase = CriarUseCase();

        var acao = async () => await useCase.ExecutarAsync(input);

        await acao.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*vazio*");
    }

    [Fact]
    public async Task Executar_PdfSemTexto_DeveLancarInvalidOperationException()
    {
        // Arrange — parser retorna zero chunks (PDF sem texto extraível)
        var conteudo = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var input = new UploadCctInputDTO(EmpresaId, "imagem.pdf", conteudo);

        _parserMock.Setup(p => p.Extrair(conteudo, "imagem.pdf", It.IsAny<int>()))
                   .Returns(Enumerable.Empty<ChunkTexto>());

        _vectorMock.Setup(v => v.GarantirColecaoAsync(EmpresaId, 768, It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var useCase = CriarUseCase();

        var acao = async () => await useCase.ExecutarAsync(input);

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Nenhum texto*");
    }

    [Fact]
    public async Task Executar_InputNulo_DeveLancarArgumentNullException()
    {
        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(null!);
        await acao.Should().ThrowAsync<ArgumentNullException>();
    }
}
