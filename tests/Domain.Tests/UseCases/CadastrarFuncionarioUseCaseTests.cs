using Application.UseCases;

namespace Domain.Tests.UseCases;

public sealed class CadastrarFuncionarioUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private readonly Mock<IFuncionarioRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private CadastrarFuncionarioUseCase CriarUseCase() =>
        new(_repoMock.Object, _uowMock.Object);

    private static CadastrarFuncionarioInputDTO InputValido() => new(
        EmpresaId: EmpresaId,
        Nome: "Ana Lima",
        Cpf: "529.982.247-25",
        Matricula: "F100",
        DataAdmissao: new DateTime(2026, 1, 10),
        HoraEntrada: "08:00",
        HoraSaida: "17:00",
        IntervaloAlmocoMinutos: 60);

    [Fact]
    public async Task Executar_DadosValidos_DeveCadastrarERetornarDTO()
    {
        _repoMock.Setup(r => r.ObterPorMatriculaAsync("F100", EmpresaId, default))
                 .ReturnsAsync((Funcionario?)null);

        var useCase = CriarUseCase();
        var result = await useCase.ExecutarAsync(InputValido());

        result.Nome.Should().Be("Ana Lima");
        result.Matricula.Should().Be("F100");
        result.Cpf.Should().Be("529.982.247-25");
        result.EmpresaId.Should().Be(EmpresaId);
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Funcionario>(), default), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task Executar_MatriculaDuplicada_DeveLancarInvalidOperationException()
    {
        var existente = Funcionario.Criar(EmpresaId, "Outro", new Cpf("529.982.247-25"), "F100",
            DateTime.UtcNow, new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1)));

        _repoMock.Setup(r => r.ObterPorMatriculaAsync("F100", EmpresaId, default))
                 .ReturnsAsync(existente);

        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(InputValido());

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*matrícula*");
    }

    [Fact]
    public async Task Executar_CpfInvalido_DeveLancarArgumentException()
    {
        var input = InputValido() with { Cpf = "111.111.111-11" };
        var useCase = CriarUseCase();

        var acao = async () => await useCase.ExecutarAsync(input);
        await acao.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Executar_InputNulo_DeveLancarArgumentNullException()
    {
        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(null!);
        await acao.Should().ThrowAsync<ArgumentNullException>();
    }
}
