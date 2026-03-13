using Application.UseCases;

namespace Domain.Tests.UseCases;

public sealed class ListarFuncionariosUseCaseTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private readonly Mock<IFuncionarioRepository> _repoMock = new();

    private ListarFuncionariosUseCase CriarUseCase() => new(_repoMock.Object);

    private static Funcionario CriarFuncionario(string nome, string matricula) =>
        Funcionario.Criar(EmpresaId, nome, new Cpf("529.982.247-25"), matricula,
            DateTime.UtcNow.AddDays(-30),
            new TurnoTrabalho(new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1)));

    [Fact]
    public async Task Executar_EmpresaComFuncionarios_DeveRetornarLista()
    {
        var lista = new List<Funcionario>
        {
            CriarFuncionario("Ana Lima", "F001"),
            CriarFuncionario("Bruno Costa", "F002")
        };

        _repoMock.Setup(r => r.ListarPorEmpresaAsync(EmpresaId, default))
                 .ReturnsAsync(lista);

        var useCase = CriarUseCase();
        var result = await useCase.ExecutarAsync(EmpresaId);

        result.Should().HaveCount(2);
        result.Should().Contain(f => f.Matricula == "F001");
        result.Should().Contain(f => f.Matricula == "F002");
    }

    [Fact]
    public async Task Executar_SemFuncionarios_DeveRetornarListaVazia()
    {
        _repoMock.Setup(r => r.ListarPorEmpresaAsync(EmpresaId, default))
                 .ReturnsAsync([]);

        var useCase = CriarUseCase();
        var result = await useCase.ExecutarAsync(EmpresaId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Executar_EmpresaIdVazio_DeveLancarArgumentException()
    {
        var useCase = CriarUseCase();
        var acao = async () => await useCase.ExecutarAsync(Guid.Empty);
        await acao.Should().ThrowAsync<ArgumentException>().WithMessage("*EmpresaId*");
    }
}
