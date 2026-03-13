using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities;

public class ContrachequeTests
{
    private static readonly Guid EmpresaId    = Guid.NewGuid();
    private static readonly Guid FechamentoId = Guid.NewGuid();
    private static readonly Guid FuncionarioId = Guid.NewGuid();

    // ─── Criar Guards ────────────────────────────────────────────────────────
    [Fact]
    public void Criar_EmpresaIdVazio_LancaArgumentException()
    {
        var act = () => Contracheque.Criar(Guid.Empty, FechamentoId, FuncionarioId, "03/2026");

        act.Should().Throw<ArgumentException>().WithMessage("*EmpresaId*");
    }

    [Fact]
    public void Criar_FechamentoIdVazio_LancaArgumentException()
    {
        var act = () => Contracheque.Criar(EmpresaId, Guid.Empty, FuncionarioId, "03/2026");

        act.Should().Throw<ArgumentException>().WithMessage("*FechamentoFolhaId*");
    }

    [Fact]
    public void Criar_FuncionarioIdVazio_LancaArgumentException()
    {
        var act = () => Contracheque.Criar(EmpresaId, FechamentoId, Guid.Empty, "03/2026");

        act.Should().Throw<ArgumentException>().WithMessage("*FuncionarioId*");
    }

    [Fact]
    public void Criar_CompetenciaNula_LancaArgumentException()
    {
        var act = () => Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "");

        act.Should().Throw<ArgumentException>();
    }

    // ─── Criar sucesso ───────────────────────────────────────────────────────
    [Fact]
    public void Criar_DadosValidos_RetornaContrachequeComIdGerado()
    {
        var c = Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "03/2026");

        c.Id.Should().NotBe(Guid.Empty);
        c.Competencia.Should().Be("03/2026");
        c.SalarioBruto.Should().Be(0m);
        c.TotalDescontos.Should().Be(0m);
        c.Itens.Should().BeEmpty();
    }

    // ─── AdicionarItem recalcula totais ───────────────────────────────────────
    [Fact]
    public void AdicionarItem_Provento_RecalculaSalarioBruto()
    {
        var c = Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "03/2026");

        c.AdicionarItem(TipoRubrica.SalarioBase, "Salário Base", 3_000.00m);

        c.SalarioBruto.Should().Be(3_000.00m);
        c.TotalDescontos.Should().Be(0m);
    }

    [Fact]
    public void AdicionarItem_Desconto_RecalculaTotalDescontos()
    {
        var c = Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "03/2026");
        c.AdicionarItem(TipoRubrica.SalarioBase, "Salário Base", 3_000.00m);
        c.AdicionarItem(TipoRubrica.DescontoInss, "INSS", 258.82m);

        c.TotalDescontos.Should().Be(258.82m);
    }

    [Fact]
    public void AdicionarItem_FgtsInformativo_NaoEntraNeSalarioBrutoNemDescontos()
    {
        var c = Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "03/2026");
        c.AdicionarItem(TipoRubrica.SalarioBase, "Salário Base", 3_000.00m);
        c.AdicionarItem(TipoRubrica.FgtsInformativo, "FGTS", 240.00m);

        c.SalarioBruto.Should().Be(3_000.00m);
        c.TotalDescontos.Should().Be(0m);
        c.FgtsPatronal.Should().Be(240.00m);
    }

    // ─── SalárioLíquido ───────────────────────────────────────────────────────
    [Fact]
    public void SalarioLiquido_SepreIgualBrutoMenosDescontos()
    {
        var c = Contracheque.Criar(EmpresaId, FechamentoId, FuncionarioId, "03/2026");
        c.AdicionarItem(TipoRubrica.SalarioBase, "Salário Base", 3_000.00m);
        c.AdicionarItem(TipoRubrica.DescontoInss, "INSS", 258.82m);
        c.AdicionarItem(TipoRubrica.DescontoIrrf, "IRRF", 0m);

        c.SalarioLiquido.Should().Be(c.SalarioBruto - c.TotalDescontos);
    }
}
