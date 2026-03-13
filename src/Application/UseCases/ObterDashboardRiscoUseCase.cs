using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;

namespace Application.UseCases;

/// <summary>
/// Agrega os alertas de anomalia do tenant no período e monta o dashboard de risco trabalhista.
/// Leitura pura — não commita nada.
/// </summary>
public sealed class ObterDashboardRiscoUseCase
{
    private const int TopFuncionariosCount = 5;

    private readonly IAlertaAnomaliaQueryRepository _queryRepo;
    private readonly IFuncionarioRepository _funcionarioRepo;

    public ObterDashboardRiscoUseCase(
        IAlertaAnomaliaQueryRepository queryRepo,
        IFuncionarioRepository funcionarioRepo)
    {
        _queryRepo = queryRepo;
        _funcionarioRepo = funcionarioRepo;
    }

    public async Task<DashboardRiscoOutputDTO> ExecutarAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (periodoFim < periodoInicio)
            throw new ArgumentException("PeriodoFim deve ser maior ou igual ao PeriodoInicio.");

        var alertas = await _queryRepo.ListarPorPeriodoAsync(empresaId, periodoInicio, periodoFim, ct);

        var totalCriticos     = alertas.Count(a => a.Gravidade == 3);
        var totalAtencao      = alertas.Count(a => a.Gravidade == 2);
        var totalInformativos = alertas.Count(a => a.Gravidade == 1);
        var totalResolvidos   = alertas.Count(a => a.Resolvido);

        // Agrupamento por tipo de anomalia
        var anomaliasPorTipo = alertas
            .GroupBy(a => a.TipoAnomalia)
            .Select(g => new AnomaliasPorTipoDTO(
                g.Key,
                g.Key.ToString(),
                g.Count(),
                g.Count(a => a.Gravidade == 3),
                g.Count(a => a.Gravidade == 2),
                g.Count(a => a.Gravidade == 1)))
            .OrderByDescending(x => x.Criticas)
            .ThenByDescending(x => x.Total)
            .ToList();

        // Top funcionários com mais alertas
        var funcionarios = await _funcionarioRepo.ListarPorEmpresaAsync(empresaId, ct);
        var nomePorId = funcionarios.ToDictionary(f => f.Id, f => f.Nome);

        var topFuncionarios = alertas
            .GroupBy(a => a.FuncionarioId)
            .Select(g => new FuncionarioRiscoDTO(
                g.Key,
                nomePorId.TryGetValue(g.Key, out var nome) ? nome : "Desconhecido",
                g.Count(),
                g.Count(a => a.Gravidade == 3)))
            .OrderByDescending(x => x.AlertasCriticos)
            .ThenByDescending(x => x.TotalAlertas)
            .Take(TopFuncionariosCount)
            .ToList();

        return new DashboardRiscoOutputDTO(
            empresaId,
            periodoInicio,
            periodoFim,
            alertas.Count,
            totalCriticos,
            totalAtencao,
            totalInformativos,
            totalResolvidos,
            anomaliasPorTipo,
            topFuncionarios);
    }
}
