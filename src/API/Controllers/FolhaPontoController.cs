using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller responsável pelas operações de auditoria de ponto e folha.
/// Rota base: /api/folhaponto
/// </summary>
[ApiController]
[Route("api/folhaponto")]
[Produces("application/json")]
public sealed class FolhaPontoController : ControllerBase
{
    private readonly UploadResumoFolhaUseCase _uploadUseCase;
    private readonly AnalisarRegistrosComIaUseCase _analisarUseCase;

    public FolhaPontoController(
        UploadResumoFolhaUseCase uploadUseCase,
        AnalisarRegistrosComIaUseCase analisarUseCase)
    {
        _uploadUseCase = uploadUseCase;
        _analisarUseCase = analisarUseCase;
    }

    /// <summary>
    /// Recebe o arquivo de resumo de folha e enfileira o processamento em background.
    /// Retorna imediatamente com 202 Accepted e o ID do processo para rastreamento.
    /// </summary>
    /// <param name="empresaId">Identificador único da empresa (multi-tenant).</param>
    /// <param name="arquivo">Arquivo de folha de ponto (form-data).</param>
    /// <param name="periodoInicio">Data de início do período (yyyy-MM-dd).</param>
    /// <param name="periodoFim">Data de fim do período (yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpPost("{empresaId:guid}/upload")]
    [ProducesResponseType(typeof(OperacaoAceitaOutputDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadResumoFolha(
        [FromRoute] Guid empresaId,
        IFormFile arquivo,
        [FromQuery] DateOnly periodoInicio,
        [FromQuery] DateOnly periodoFim,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new ArgumentException("Nenhum arquivo foi enviado ou o arquivo está vazio.", nameof(arquivo));

        var input = new UploadResumoFolhaInputDTO(
            empresaId,
            arquivo.FileName,
            arquivo.OpenReadStream(),
            periodoInicio,
            periodoFim);

        var resultado = await _uploadUseCase.ExecutarAsync(input, cancellationToken);

        return Accepted(resultado);
    }

    /// <summary>
    /// Dispara a análise de registros de ponto de um funcionário via IA.
    /// Retorna as anomalias detectadas para o período informado.
    /// </summary>
    /// <param name="empresaId">Identificador único da empresa (multi-tenant).</param>
    /// <param name="funcionarioId">Identificador único do funcionário.</param>
    /// <param name="periodoInicio">Data de início do período (yyyy-MM-dd).</param>
    /// <param name="periodoFim">Data de fim do período (yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpGet("{empresaId:guid}/funcionarios/{funcionarioId:guid}/anomalias")]
    [ProducesResponseType(typeof(IEnumerable<AlertaAnomaliaOutputDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AnalisarAnomalias(
        [FromRoute] Guid empresaId,
        [FromRoute] Guid funcionarioId,
        [FromQuery] DateOnly periodoInicio,
        [FromQuery] DateOnly periodoFim,
        CancellationToken cancellationToken)
    {
        var input = new AnalisarRegistrosInputDTO(
            empresaId,
            funcionarioId,
            periodoInicio,
            periodoFim);

        var alertas = await _analisarUseCase.ExecutarAsync(input, cancellationToken);

        return Ok(alertas);
    }
}
