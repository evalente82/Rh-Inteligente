using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Endpoints analíticos do Dashboard de Risco Trabalhista (M5).
/// Rota base: /api/{empresaId}/dashboard
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/dashboard")]
[Produces("application/json")]
[Authorize(Roles = "Dono,Gestor")]
public sealed class DashboardController : ControllerBase
{
    private readonly ObterDashboardRiscoUseCase _dashboardUseCase;
    private readonly ObterIndiceConformidadeUseCase _indiceUseCase;

    public DashboardController(
        ObterDashboardRiscoUseCase dashboardUseCase,
        ObterIndiceConformidadeUseCase indiceUseCase)
    {
        _dashboardUseCase = dashboardUseCase;
        _indiceUseCase = indiceUseCase;
    }

    /// <summary>
    /// Retorna o dashboard completo de risco trabalhista com anomalias por tipo
    /// e ranking dos funcionários com mais alertas no período.
    /// </summary>
    [HttpGet("risco")]
    [ProducesResponseType(typeof(DashboardRiscoOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterDashboardRisco(
        [FromRoute] Guid empresaId,
        [FromQuery] DateOnly periodoInicio,
        [FromQuery] DateOnly periodoFim,
        CancellationToken ct)
    {
        var resultado = await _dashboardUseCase.ExecutarAsync(empresaId, periodoInicio, periodoFim, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Retorna o Índice de Conformidade Trabalhista (0-100) com classificação Verde/Amarelo/Vermelho.
    /// </summary>
    [HttpGet("conformidade")]
    [ProducesResponseType(typeof(IndiceConformidadeOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ObterIndiceConformidade(
        [FromRoute] Guid empresaId,
        [FromQuery] DateOnly periodoInicio,
        [FromQuery] DateOnly periodoFim,
        CancellationToken ct)
    {
        var resultado = await _indiceUseCase.ExecutarAsync(empresaId, periodoInicio, periodoFim, ct);
        return Ok(resultado);
    }
}
