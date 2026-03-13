using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Tests.Entities;

public class AdmissaoTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();
    private static readonly DateOnly DataAdmissao = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

    private static Endereco EnderecoValido() => new(
        Logradouro: "Rua das Flores",
        Numero: "123",
        Bairro: "Centro",
        Cidade: "São Paulo",
        Uf: "SP",
        Cep: "01310100");

    [Fact]
    public void Criar_DadosValidos_DeveInstanciarAdmissao()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista", 5000m,
            RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        admissao.Cargo.Should().Be("Analista");
        admissao.SalarioBase.Should().Be(5000m);
        admissao.Regime.Should().Be(RegimeContratacao.Clt);
        admissao.Ativa.Should().BeTrue();
        admissao.EmpresaId.Should().Be(EmpresaId);
        admissao.FuncionarioId.Should().Be(FuncionarioId);
    }

    [Fact]
    public void Criar_EmpresaIdVazio_DeveLancarArgumentException()
    {
        var acao = () => Admissao.Criar(Guid.Empty, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        acao.Should().Throw<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Fact]
    public void Criar_SalarioZero_DeveLancarArgumentException()
    {
        var acao = () => Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            0m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        acao.Should().Throw<ArgumentException>().WithMessage("*Salário*");
    }

    [Fact]
    public void Criar_SalarioNegativo_DeveLancarArgumentException()
    {
        var acao = () => Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            -1m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        acao.Should().Throw<ArgumentException>().WithMessage("*Salário*");
    }

    [Fact]
    public void Criar_CargoVazio_DeveLancarArgumentException()
    {
        var acao = () => Admissao.Criar(EmpresaId, FuncionarioId, "   ",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Demitir_DataValida_DeveEncerrarAdmissao()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        var dataDemissao = DataAdmissao.AddDays(90);
        admissao.Demitir(dataDemissao);

        admissao.Ativa.Should().BeFalse();
        admissao.DataDemissao.Should().Be(dataDemissao);
    }

    [Fact]
    public void Demitir_DataAnteriorAdmissao_DeveLancarInvalidOperationException()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        var acao = () => admissao.Demitir(DataAdmissao.AddDays(-1));
        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Demitir_AdmissaoJaEncerrada_DeveLancarInvalidOperationException()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        admissao.Demitir(DataAdmissao.AddDays(10));

        var acao = () => admissao.Demitir(DataAdmissao.AddDays(20));
        acao.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReajustarSalario_ValorValido_DeveAtualizar()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        admissao.ReajustarSalario(5500m);

        admissao.SalarioBase.Should().Be(5500m);
    }

    [Fact]
    public void ReajustarSalario_AdmissaoEncerrada_DeveLancarInvalidOperationException()
    {
        var admissao = Admissao.Criar(EmpresaId, FuncionarioId, "Analista",
            5000m, RegimeContratacao.Clt, DataAdmissao, EnderecoValido());

        admissao.Demitir(DataAdmissao.AddDays(10));

        var acao = () => admissao.ReajustarSalario(6000m);
        acao.Should().Throw<InvalidOperationException>();
    }
}
