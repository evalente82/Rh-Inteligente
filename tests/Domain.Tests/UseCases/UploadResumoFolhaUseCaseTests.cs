using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;

namespace Domain.Tests.UseCases;

public sealed class UploadResumoFolhaUseCaseTests
{
    private readonly Mock<IArmazenamentoArquivoService> _armazenamentoMock = new();
    private readonly Mock<IAnalisadorBackgroundService> _backgroundMock = new();

    private UploadResumoFolhaUseCase CriarUseCase() =>
        new(_armazenamentoMock.Object, _backgroundMock.Object);

    // Dados auxiliares reutilizáveis
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly DateOnly PeriodoInicio = new(2026, 3, 1);
    private static readonly DateOnly PeriodoFim = new(2026, 3, 31);

    // =========================================================
    // Cenário: fluxo feliz — retorna 202 com ProcessoId
    // =========================================================

    [Fact]
    public async Task ExecutarAsync_QuandoDadosValidos_DeveRetornarOperacaoAceita()
    {
        // Arrange
        var processoId = Guid.NewGuid();
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _armazenamentoMock
            .Setup(s => s.SalvarAsync(It.IsAny<string>(), It.IsAny<Stream>(),
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/arquivos/empresa/ponto_marco.csv");

        _backgroundMock
            .Setup(b => b.EnfileirarAnaliseAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processoId);

        var input = new UploadResumoFolhaInputDTO(EmpresaId, "ponto_marco.csv",
            stream, PeriodoInicio, PeriodoFim);

        // Act
        var resultado = await CriarUseCase().ExecutarAsync(input);

        // Assert
        resultado.Should().NotBeNull();
        resultado.ProcessoId.Should().Be(processoId);
        resultado.Mensagem.Should().Contain(processoId.ToString());
    }

    [Fact]
    public async Task ExecutarAsync_QuandoDadosValidos_DeveChamarSalvarEEnfileirarUmaVezCadaUm()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        _armazenamentoMock
            .Setup(s => s.SalvarAsync(It.IsAny<string>(), stream, EmpresaId, default))
            .ReturnsAsync("/path/arquivo");
        _backgroundMock
            .Setup(b => b.EnfileirarAnaliseAsync(EmpresaId, Guid.Empty,
                PeriodoInicio, PeriodoFim, default))
            .ReturnsAsync(Guid.NewGuid());

        var input = new UploadResumoFolhaInputDTO(EmpresaId, "ponto.csv",
            stream, PeriodoInicio, PeriodoFim);

        // Act
        await CriarUseCase().ExecutarAsync(input);

        // Assert — verifica que as dependências foram chamadas exatamente uma vez
        _armazenamentoMock.Verify(s => s.SalvarAsync(
            "ponto.csv", stream, EmpresaId, default), Times.Once);

        _backgroundMock.Verify(b => b.EnfileirarAnaliseAsync(
            EmpresaId, Guid.Empty, PeriodoInicio, PeriodoFim, default), Times.Once);
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
        var input = new UploadResumoFolhaInputDTO(Guid.Empty, "ponto.csv",
            new MemoryStream([1]), PeriodoInicio, PeriodoFim);

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecutarAsync_QuandoNomeArquivoVazio_DeveLancarArgumentException(string nome)
    {
        var input = new UploadResumoFolhaInputDTO(EmpresaId, nome,
            new MemoryStream([1]), PeriodoInicio, PeriodoFim);

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExecutarAsync_QuandoArquivoVazio_DeveLancarArgumentException()
    {
        var input = new UploadResumoFolhaInputDTO(EmpresaId, "ponto.csv",
            new MemoryStream([]), PeriodoInicio, PeriodoFim);

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*arquivo*");
    }

    [Fact]
    public async Task ExecutarAsync_QuandoPeriodoFimAnteriorAoInicio_DeveLancarArgumentException()
    {
        // PeriodoInicio = 31/03, PeriodoFim = 01/03 → invertido
        var input = new UploadResumoFolhaInputDTO(EmpresaId, "ponto.csv",
            new MemoryStream([1]),
            new DateOnly(2026, 3, 31),
            new DateOnly(2026, 3, 1));

        var act = async () => await CriarUseCase().ExecutarAsync(input);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*período*");
    }
}
