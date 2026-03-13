using Application.UseCases;

namespace Domain.Tests.UseCases;

public sealed class AdmitirFuncionarioUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();

    private readonly Mock<IFuncionarioRepository> _funcRepoMock = new();
    private readonly Mock<IAdmissaoRepository> _admRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private AdmitirFuncionarioUseCase CriarUseCase() =>
        new(_funcRepoMock.Object, _admRepoMock.Object, _uowMock.Object);

    private static Funcionario FuncionarioPadrao() =>
        Funcionario.Criar(EmpresaId, "Carlos Souza", new Cpf("529.982.247-25"), "F200",
            DateTime.UtcNow.AddDays(-60),
            new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1)));

    private static AdmitirFuncionarioInputDTO InputValido() => new(
        EmpresaId: EmpresaId,
        FuncionarioId: FuncionarioId,
        Cargo: "Analista de Sistemas",
        SalarioBase: 6000m,
        Regime: RegimeContratacao.Clt,
        DataAdmissao: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-60)),
        Logradouro: "Rua das Flores",
        NumeroEndereco: "10",
        Bairro: "Centro",
        Cidade: "São Paulo",
        Uf: "SP",
        Cep: "01310100");

    [Fact]
    public async Task Executar_DadosValidos_DeveAdmitirERetornarDTO()
    {
        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync(FuncionarioPadrao());
        _admRepoMock.Setup(r => r.ObterAdmissaoAtivaAsync(FuncionarioId, EmpresaId, default))
                    .ReturnsAsync((Admissao?)null);

        var useCase = CriarUseCase();
        var result = await useCase.ExecutarAsync(InputValido());

        result.Cargo.Should().Be("Analista de Sistemas");
        result.SalarioBase.Should().Be(6000m);
        result.Regime.Should().Be(RegimeContratacao.Clt);
        _admRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<Admissao>(), default), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task Executar_FuncionarioNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync((Funcionario?)null);

        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(InputValido());

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Executar_AdmissaoJaAtiva_DeveLancarInvalidOperationException()
    {
        var admissaoAtiva = Admissao.Criar(EmpresaId, FuncionarioId, "Cargo", 5000m,
            RegimeContratacao.Clt, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)),
            new Endereco("Rua A", "1", "Bairro", "Cidade", "SP", "01310100"));

        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync(FuncionarioPadrao());
        _admRepoMock.Setup(r => r.ObterAdmissaoAtivaAsync(FuncionarioId, EmpresaId, default))
                    .ReturnsAsync(admissaoAtiva);

        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(InputValido());

        await acao.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*admissão ativa*");
    }
}
