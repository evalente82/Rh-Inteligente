using Domain.Services;

namespace Domain.Tests.Services;

public class CalculoEncargosFolhaServiceTests
{
    private readonly CalculoEncargosFolhaService _sut = new();

    // ─── INSS Faixa 1 ─────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioMinimo_InssNaPrimeirFaixa_7e5Porcento()
    {
        // Salário mínimo 2024: R$ 1.412,00 → INSS = 7,5% = R$ 105,90
        var resultado = _sut.Calcular(1_412.00m);

        resultado.InssDevido.Should().Be(105.90m);
    }

    // ─── INSS Faixa 2 ─────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioNaSegundaFaixa_InssProgressivo()
    {
        // R$ 2.000,00 → faixa 1: (1412 * 0,075 = 105,90) + faixa 2: (588 * 0,09 = 52,92) = 158,82
        var resultado = _sut.Calcular(2_000.00m);

        resultado.InssDevido.Should().Be(158.82m);
    }

    // ─── INSS Faixa 3 ─────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioNaTerceiraFaixa_InssProgressivo()
    {
        // R$ 3.000,00 → faixas 1+2+3:
        //   F1: 1412,00 × 7,5% = 105,90
        //   F2: (2666,68-1412,00) × 9%  = 112,92
        //   F3: (3000,00-2666,68) × 12% = 39,998 → total = 258,82
        var resultado = _sut.Calcular(3_000.00m);

        resultado.InssDevido.Should().Be(258.82m);
    }

    // ─── INSS Teto ────────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioAcimaDoTeto_InssLimitadoAoMaximo()
    {
        // Acima de R$ 7.786,02 → teto = R$ 908,86
        var resultado = _sut.Calcular(15_000.00m);

        resultado.InssDevido.Should().Be(908.86m);
    }

    // ─── IRRF Isento ─────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioBaixo_IrrfZero_Isento()
    {
        // R$ 1.412,00 → base IRRF após INSS = 1.306,10 → isento (< 2.824)
        var resultado = _sut.Calcular(1_412.00m);

        resultado.IrrfDevido.Should().Be(0m);
    }

    // ─── IRRF Tributado ───────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioAlto_IrrfDevidoPositivo()
    {
        // R$ 5.000,00 → INSS ≈ R$ 479,05 → base IRRF ≈ 4.520,95 → 15% - 492,87 = R$ 185,27
        var resultado = _sut.Calcular(5_000.00m);

        resultado.IrrfDevido.Should().BeGreaterThan(0m);
    }

    // ─── FGTS ─────────────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioQualquer_FgtsSempre8PorCento()
    {
        var resultado = _sut.Calcular(3_000.00m);

        resultado.FgtsPatronal.Should().Be(Math.Round(3_000.00m * 0.08m, 2));
    }

    // ─── Salário Líquido ──────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioLiquido_IgualBrutoMenosInssEIrrf()
    {
        var resultado = _sut.Calcular(4_000.00m);

        var esperado = resultado.SalarioBruto - resultado.InssDevido - resultado.IrrfDevido;
        resultado.SalarioLiquido.Should().Be(esperado);
    }

    // ─── Guard ───────────────────────────────────────────────────────────────
    [Fact]
    public void Calcular_SalarioZeroOuNegativo_LancaArgumentException()
    {
        var act = () => _sut.Calcular(0m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*bruto*");
    }
}
