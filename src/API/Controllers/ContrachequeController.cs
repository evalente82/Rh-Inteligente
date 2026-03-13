using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Endpoints do Módulo 7 — Geração e consulta de contracheques (holerites) por fechamento de folha.
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/contracheque")]
[Authorize(Roles = "Dono,Gestor")]
public sealed class ContrachequeController : ControllerBase
{
    // ─── POST api/{empresaId}/contracheque/{fechamentoId}/gerar ────────────────
    /// <summary>
    /// Gera os contracheques de todos os funcionários ativos vinculados ao fechamento de folha.
    /// A operação é idempotente: funcionários que já possuem contracheque para o fechamento
    /// informado são ignorados silenciosamente.
    /// </summary>
    [HttpPost("{fechamentoId:guid}/gerar")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ContrachequeOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Gerar(
        [FromRoute] Guid empresaId,
        [FromRoute] Guid fechamentoId,
        [FromServices] GerarContrachequeUseCase useCase,
        CancellationToken ct)
    {
        var input = new GerarContrachequeInputDTO(empresaId, fechamentoId);
        var resultado = await useCase.ExecutarAsync(input, ct);
        return Ok(resultado);
    }

    // ─── GET api/{empresaId}/contracheque/{id} ─────────────────────────────────
    /// <summary>
    /// Obtém um contracheque específico pelo seu identificador único.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContrachequeOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        [FromRoute] Guid id,
        [FromServices] ObterContrachequeUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.ExecutarAsync(id, ct);
        return Ok(resultado);
    }

    // ─── GET api/{empresaId}/contracheque/fechamento/{fechamentoId} ────────────
    /// <summary>
    /// Lista todos os contracheques gerados para um fechamento de folha.
    /// </summary>
    [HttpGet("fechamento/{fechamentoId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyCollection<ContrachequeOutputDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorFechamento(
        [FromRoute] Guid fechamentoId,
        [FromServices] ListarContrachequesFolhaUseCase useCase,
        CancellationToken ct)
    {
        var resultado = await useCase.ExecutarAsync(fechamentoId, ct);
        return Ok(resultado);
    }
}
