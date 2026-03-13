using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Calcula o Índice de Conformidade Trabalhista do tenant no período.
/// Fórmula: índice = (1 - (críticos / max(total,1))) * 100, clampado entre 0 e 100.
/// </summary>
public sealed class ObterIndiceConformidadeUseCase
{
    private readonly IAlertaAnomaliaQueryRepository _queryRepo;
    private readonly IFuncionarioRepository _funcionarioRepo;

    public ObterIndiceConformidadeUseCase(
        IAlertaAnomaliaQueryRepository queryRepo,
        IFuncionarioRepository funcionarioRepo)
    {
        _queryRepo = queryRepo;
        _funcionarioRepo = funcionarioRepo;
    }

    public async Task<IndiceConformidadeOutputDTO> ExecutarAsync(
        Guid empresaId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken ct = default)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        if (periodoFim < periodoInicio)
            throw new ArgumentException("PeriodoFim deve ser maior ou igual ao PeriodoInicio.");

        var alertas     = await _queryRepo.ListarPorPeriodoAsync(empresaId, periodoInicio, periodoFim, ct);
        var funcionarios = await _funcionarioRepo.ListarPorEmpresaAsync(empresaId, ct);

        var totalFuncionarios = funcionarios.Count(f => f.Ativo);
        var criticos = alertas.Count(a => a.Gravidade == 3 && !a.Resolvido);
        var total    = alertas.Count(a => !a.Resolvido);

        // Índice: penaliza proporcionalmente pelos alertas críticos não resolvidos
        double indice = total == 0
            ? 100.0
            : Math.Round((1.0 - (criticos / (double)total)) * 100.0, 1);

        indice = Math.Clamp(indice, 0, 100);

        var classificacao = indice switch
        {
            >= 90  => "Verde",
            >= 70  => "Amarelo",
            _      => "Vermelho"
        };

        var funcionariosComAlertas = alertas
            .Select(a => a.FuncionarioId)
            .Distinct()
            .Count();

        return new IndiceConformidadeOutputDTO(
            empresaId,
            periodoInicio,
            periodoFim,
            indice,
            classificacao,
            totalFuncionarios,
            funcionariosComAlertas,
            totalFuncionarios - funcionariosComAlertas);
    }
}
