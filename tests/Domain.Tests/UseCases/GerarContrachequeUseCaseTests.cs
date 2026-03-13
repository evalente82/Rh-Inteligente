using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;

namespace Domain.Tests.UseCases;

public class GerarContrachequeUseCaseTests
{
    // ─── Mocks compartilhados ─────────────────────────────────────────────────
    private readonly Mock<IContrachequeRepository>  _contrachequeRepo  = new();
    private readonly Mock<IFechamentoFolhaRepository> _fechamentoRepo  = new();
    private readonly Mock<IFuncionarioRepository>    _funcionarioRepo  = new();
    private readonly Mock<IAdmissaoRepository>       _admissaoRepo     = new();
    private readonly Mock<IUnitOfWork>               _uow              = new();

    private readonly CalculoEncargosFolhaService _calculoEncargos = new();

    private GerarContrachequeUseCase CriarSut() =>
        new(_contrachequeRepo.Object,
            _fechamentoRepo.Object,
            _funcionarioRepo.Object,
            _admissaoRepo.Object,
            _calculoEncargos,
            _uow.Object);

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static Funcionario CriarFuncionario(Guid empresaId)
    {
        var cpf = Cpf.TentarCriar("529.982.247-25")!;
        var turno = new TurnoTrabalho(
            new TimeOnly(8, 0),
            new TimeOnly(17, 0),
            TimeSpan.FromHours(1));
        return Funcionario.Criar(empresaId, "João Teste", cpf, "MAT001",
            DateTime.Today.AddYears(-2), turno);
    }

    private static FechamentoFolha CriarFechamento(Guid empresaId) =>
        FechamentoFolha.Abrir(empresaId,
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31));

    private static Admissao CriarAdmissao(Guid empresaId, Guid funcionarioId)
    {
        var endereco = new Endereco("Rua A", "100", "Centro", "São Paulo", "SP", "01001000", null);
        return Admissao.Criar(empresaId, funcionarioId, "Analista",
            3_000.00m, RegimeContratacao.Clt,
            new DateOnly(2024, 1, 2), endereco);
    }

    // ─── EmpresaId vazio → ArgumentException ──────────────────────────────────
    [Fact]
    public async Task ExecutarAsync_EmpresaIdVazio_LancaArgumentException()
    {
        var sut = CriarSut();
        var input = new GerarContrachequeInputDTO(Guid.Empty, Guid.NewGuid());

        var act = () => sut.ExecutarAsync(input);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*EmpresaId*");
    }

    // ─── FechamentoFolhaId vazio → ArgumentException ───────────────────────────
    [Fact]
    public async Task ExecutarAsync_FechamentoIdVazio_LancaArgumentException()
    {
        var sut = CriarSut();
        var input = new GerarContrachequeInputDTO(Guid.NewGuid(), Guid.Empty);

        var act = () => sut.ExecutarAsync(input);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*FechamentoFolhaId*");
    }

    // ─── Fechamento inexistente → KeyNotFoundException ─────────────────────────
    [Fact]
    public async Task ExecutarAsync_FechamentoNaoEncontrado_LancaKeyNotFoundException()
    {
        var sut = CriarSut();
        var input = new GerarContrachequeInputDTO(Guid.NewGuid(), Guid.NewGuid());

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(input.FechamentoFolhaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FechamentoFolha?)null);

        var act = () => sut.ExecutarAsync(input);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ─── Sem funcionários ativos → retorna lista vazia ─────────────────────────
    [Fact]
    public async Task ExecutarAsync_SemFuncionariosAtivos_RetornaListaVazia()
    {
        var empresaId = Guid.NewGuid();
        var fechamento = CriarFechamento(empresaId);
        var input = new GerarContrachequeInputDTO(empresaId, fechamento.Id);

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(fechamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fechamento);

        _funcionarioRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CriarSut();
        var resultado = await sut.ExecutarAsync(input);

        resultado.Should().BeEmpty();
        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── Idempotência: contracheque já existe → pula o funcionário ──────────────
    [Fact]
    public async Task ExecutarAsync_ContrachequeJaExistente_NaoGeraSegundaVez()
    {
        var empresaId    = Guid.NewGuid();
        var fechamento   = CriarFechamento(empresaId);
        var funcionario  = CriarFuncionario(empresaId);
        var input        = new GerarContrachequeInputDTO(empresaId, fechamento.Id);

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(fechamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fechamento);
        _funcionarioRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([funcionario]);
        _contrachequeRepo.Setup(r => r.ExistePorFuncionarioEFechamentoAsync(
                funcionario.Id, fechamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = CriarSut();
        var resultado = await sut.ExecutarAsync(input);

        resultado.Should().BeEmpty();
        _contrachequeRepo.Verify(r => r.AdicionarAsync(It.IsAny<Contracheque>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ─── Geração com sucesso: INSS calculado, rubrica SalarioBase presente ─────
    [Fact]
    public async Task ExecutarAsync_FuncionarioAtivo_GeraContrachequeComInss()
    {
        var empresaId   = Guid.NewGuid();
        var fechamento  = CriarFechamento(empresaId);
        var funcionario = CriarFuncionario(empresaId);
        var admissao    = CriarAdmissao(empresaId, funcionario.Id);
        var input       = new GerarContrachequeInputDTO(empresaId, fechamento.Id);

        _fechamentoRepo.Setup(r => r.ObterPorIdAsync(fechamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fechamento);
        _funcionarioRepo.Setup(r => r.ListarPorEmpresaAsync(empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([funcionario]);
        _contrachequeRepo.Setup(r => r.ExistePorFuncionarioEFechamentoAsync(
                funcionario.Id, fechamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _admissaoRepo.Setup(r => r.ObterAdmissaoAtivaAsync(
                funcionario.Id, empresaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(admissao);
        _contrachequeRepo.Setup(r => r.AdicionarAsync(It.IsAny<Contracheque>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var sut = CriarSut();
        var resultado = await sut.ExecutarAsync(input);

        resultado.Should().HaveCount(1);

        var contracheque = resultado.First();
        contracheque.SalarioBruto.Should().Be(3_000.00m);
        contracheque.Itens.Should().Contain(i => i.Tipo == TipoRubrica.SalarioBase.ToString());
        contracheque.Itens.Should().Contain(i => i.Tipo == TipoRubrica.DescontoInss.ToString());
        contracheque.FgtsPatronal.Should().Be(Math.Round(3_000.00m * 0.08m, 2));

        _uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
