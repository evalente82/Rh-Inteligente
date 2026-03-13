using Application.UseCases;

namespace Domain.Tests.UseCases;

public sealed class DemitirFuncionarioUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();

    private readonly Mock<IFuncionarioRepository> _funcRepoMock = new();
    private readonly Mock<IAdmissaoRepository> _admRepoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private DemitirFuncionarioUseCase CriarUseCase() =>
        new(_funcRepoMock.Object, _admRepoMock.Object, _uowMock.Object);

    private static Funcionario FuncionarioAtivo() =>
        Funcionario.Criar(EmpresaId, "Lucas Ramos", new Cpf("529.982.247-25"), "F300",
            new DateTime(2024, 1, 1),
            new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1)));

    [Fact]
    public async Task Executar_FuncionarioAtivo_DeveDemitir()
    {
        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync(FuncionarioAtivo());
        _admRepoMock.Setup(r => r.ObterAdmissaoAtivaAsync(FuncionarioId, EmpresaId, default))
                    .ReturnsAsync((Admissao?)null);

        var useCase = CriarUseCase();
        await useCase.ExecutarAsync(EmpresaId, FuncionarioId, new DateTime(2026, 3, 13));

        _uowMock.Verify(u => u.CommitAsync(default), Times.Once);
        _funcRepoMock.Verify(r => r.Atualizar(It.IsAny<Funcionario>()), Times.Once);
    }

    [Fact]
    public async Task Executar_FuncionarioComAdmissaoAtiva_DeveDemitirAdmissaoTambem()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Dev", 5000m,
            RegimeContratacao.Clt, new DateOnly(2024, 1, 1),
            new Endereco("Rua B", "2", "Bairro", "Cidade", "SP", "01310100"));

        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync(FuncionarioAtivo());
        _admRepoMock.Setup(r => r.ObterAdmissaoAtivaAsync(FuncionarioId, EmpresaId, default))
                    .ReturnsAsync(admissao);

        var useCase = CriarUseCase();
        await useCase.ExecutarAsync(EmpresaId, FuncionarioId, new DateTime(2026, 3, 13));

        _admRepoMock.Verify(r => r.Atualizar(admissao), Times.Once);
        admissao.Ativa.Should().BeFalse();
    }

    [Fact]
    public async Task Executar_FuncionarioNaoEncontrado_DeveLancarKeyNotFoundException()
    {
        _funcRepoMock.Setup(r => r.ObterPorIdAsync(FuncionarioId, EmpresaId, default))
                     .ReturnsAsync((Funcionario?)null);

        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(EmpresaId, FuncionarioId, DateTime.UtcNow);

        await acao.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Executar_EmpresaIdVazio_DeveLancarArgumentException()
    {
        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(Guid.Empty, FuncionarioId, DateTime.UtcNow);
        await acao.Should().ThrowAsync<ArgumentException>().WithMessage("*EmpresaId*");
    }
}
