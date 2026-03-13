namespace Domain.Services;

/// <summary>
/// Resultado do cálculo de encargos sobre o salário de um funcionário.
/// </summary>
/// <param name="SalarioBruto">Base de cálculo (salário + horas extras).</param>
/// <param name="InssDevido">Desconto INSS calculado pela tabela progressiva 2024.</param>
/// <param name="BaseCalculoIrrf">Salário bruto menos INSS (base do IRRF).</param>
/// <param name="IrrfDevido">Imposto de renda retido na fonte (tabela progressiva 2024).</param>
/// <param name="FgtsPatronal">Contribuição ao FGTS (8% do bruto — encargo patronal).</param>
/// <param name="SalarioLiquido">Bruto - INSS - IRRF.</param>
public sealed record ResultadoCalculoEncargos(
    decimal SalarioBruto,
    decimal InssDevido,
    decimal BaseCalculoIrrf,
    decimal IrrfDevido,
    decimal FgtsPatronal,
    decimal SalarioLiquido);

/// <summary>
/// Domain Service que calcula INSS, IRRF e FGTS conforme tabelas 2024 vigentes.
/// Regras: tabela progressiva INSS (Portaria MPS 1.730/2024), tabela IRRF 2024 e FGTS 8%.
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed class CalculoEncargosFolhaService
{
    // ─── Tabela progressiva INSS 2024 ─────────────────────────────────────────
    // Faixas: até R$ 1.412,00 → 7,5% | até R$ 2.666,68 → 9% | até R$ 4.000,03 → 12% | até R$ 7.786,02 → 14%
    // Acima do teto (R$ 7.786,02): desconto máximo de R$ 908,86
    private static readonly (decimal LimiteMaximo, decimal Aliquota, decimal DeducaoFaixa)[] _faixasInss =
    [
        (  1_412.00m, 0.075m,    0.00m),
        (  2_666.68m, 0.090m,   21.18m),
        (  4_000.03m, 0.120m,  101.18m),
        (  7_786.02m, 0.140m,  181.18m)
    ];
    private const decimal InssMaximo = 908.86m;

    // ─── Tabela progressiva IRRF 2024 ─────────────────────────────────────────
    // Base: base após dedução do INSS e R$ 189,59 por dependente (sem dependentes aqui)
    // Isento: até R$ 2.824,00 | 7,5% até R$ 3.751,05 | 15% até R$ 4.664,68 | 22,5% até R$ 6.101,06 | 27,5% acima
    private static readonly (decimal LimiteMaximo, decimal Aliquota, decimal ParcelaDeducao)[] _faixasIrrf =
    [
        (2_824.00m, 0.000m,    0.00m),
        (3_751.05m, 0.075m,  211.80m),
        (4_664.68m, 0.150m,  492.87m),
        (6_101.06m, 0.225m,  840.47m),
        (decimal.MaxValue, 0.275m, 1_145.80m)
    ];

    private const decimal AliquotaFgts = 0.08m;

    /// <summary>
    /// Calcula INSS, IRRF e FGTS para o salário bruto informado.
    /// </summary>
    /// <param name="salarioBruto">
    /// Soma do salário base + horas extras do período.
    /// Deve ser maior que zero.
    /// </param>
    public ResultadoCalculoEncargos Calcular(decimal salarioBruto)
    {
        if (salarioBruto <= 0)
            throw new ArgumentException("Salário bruto deve ser maior que zero.", nameof(salarioBruto));

        var inss     = CalcularInss(salarioBruto);
        var baseIrrf = salarioBruto - inss;
        var irrf     = CalcularIrrf(baseIrrf);
        var fgts     = Math.Round(salarioBruto * AliquotaFgts, 2);
        var liquido  = salarioBruto - inss - irrf;

        return new ResultadoCalculoEncargos(
            SalarioBruto   : salarioBruto,
            InssDevido      : inss,
            BaseCalculoIrrf : baseIrrf,
            IrrfDevido      : irrf,
            FgtsPatronal    : fgts,
            SalarioLiquido  : liquido);
    }

    // ─── INSS: tabela progressiva por faixas (cálculo acumulado) ─────────────
    private static decimal CalcularInss(decimal salarioBruto)
    {
        if (salarioBruto > 7_786.02m)
            return InssMaximo;

        decimal totalInss = 0m;
        decimal limiteAnterior = 0m;

        foreach (var (limiteMaximo, aliquota, _) in _faixasInss)
        {
            if (salarioBruto <= limiteAnterior) break;

            var baseNaFaixa = Math.Min(salarioBruto, limiteMaximo) - limiteAnterior;
            totalInss += baseNaFaixa * aliquota;
            limiteAnterior = limiteMaximo;
        }

        return Math.Round(totalInss, 2);
    }

    // ─── IRRF: tabela progressiva com parcela a deduzir ──────────────────────
    private static decimal CalcularIrrf(decimal baseCalculo)
    {
        if (baseCalculo <= 0) return 0m;

        foreach (var (limiteMaximo, aliquota, parcelaDeducao) in _faixasIrrf)
        {
            if (baseCalculo <= limiteMaximo)
            {
                var irrf = (baseCalculo * aliquota) - parcelaDeducao;
                return irrf < 0 ? 0m : Math.Round(irrf, 2);
            }
        }

        // Nunca deve chegar aqui (última faixa usa decimal.MaxValue)
        return 0m;
    }
}
