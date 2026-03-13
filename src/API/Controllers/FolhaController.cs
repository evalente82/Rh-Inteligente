using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Endpoints de fechamento de folha de ponto (M6).
/// Rota base: /api/{empresaId}/folha
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/folha")]
[Produces("application/json")]
[Authorize(Roles = "Dono,Gestor")]
public sealed class FolhaController : ControllerBase
{
    private readonly FecharFolhaUseCase _fecharFolhaUseCase;
    private readonly GerarRelatorioFolhaUseCase _relatorioUseCase;

    public FolhaController(
        FecharFolhaUseCase fecharFolhaUseCase,
        GerarRelatorioFolhaUseCase relatorioUseCase)
    {
        _fecharFolhaUseCase = fecharFolhaUseCase;
        _relatorioUseCase = relatorioUseCase;
    }

    /// <summary>
    /// Consolida o período, calcula horas extras/descontos e fecha a folha.
    /// </summary>
    /// <response code="200">Fechamento criado com sucesso.</response>
    /// <response code="400">Período inválido ou EmpresaId vazio.</response>
    /// <response code="422">Já existe fechamento para o período.</response>
    [HttpPost("fechar")]
    [ProducesResponseType(typeof(FechamentoFolhaOutputDTO), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Fechar(
        Guid empresaId,
        [FromBody] FecharFolhaInputDTO input,
        CancellationToken ct)
    {
        // Garante que o empresaId da rota prevaleça sobre o body
        var inputComEmpresa = input with { EmpresaId = empresaId };
        var resultado = await _fecharFolhaUseCase.ExecutarAsync(inputComEmpresa, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Gera (ou regera) o relatório narrativo de IA de um fechamento existente.
    /// </summary>
    /// <response code="200">Relatório gerado com sucesso.</response>
    /// <response code="404">Fechamento não encontrado.</response>
    [HttpGet("{fechamentoId:guid}/relatorio")]
    [ProducesResponseType(typeof(FechamentoFolhaOutputDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterRelatorio(
        Guid fechamentoId,
        CancellationToken ct)
    {
        var resultado = await _relatorioUseCase.ExecutarAsync(fechamentoId, ct);
        return Ok(resultado);
    }
}
