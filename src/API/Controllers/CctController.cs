using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de Convenções Coletivas (CCT).
/// As regras extraídas são indexadas no Qdrant para uso no pipeline RAG de auditoria.
/// Rota base: /api/{empresaId}/cct
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/cct")]
[Produces("application/json")]
[Authorize(Roles = "Dono,Gestor")]
public sealed class CctController : ControllerBase
{
    private readonly ProcessarCctUseCase _processarCctUseCase;
    private readonly GerarHoleriteNarrativoUseCase _holeriteUseCase;

    public CctController(
        ProcessarCctUseCase processarCctUseCase,
        GerarHoleriteNarrativoUseCase holeriteUseCase)
    {
        _processarCctUseCase = processarCctUseCase;
        _holeriteUseCase = holeriteUseCase;
    }

    /// <summary>
    /// Recebe o PDF da Convenção Coletiva e indexa as regras no Qdrant para o tenant.
    /// O processamento (extração → embeddings → Qdrant) ocorre de forma síncrona mas isolada.
    /// Retorna 202 Accepted imediatamente após enfileirar o trabalho.
    /// </summary>
    /// <param name="empresaId">Identificador único da empresa (multi-tenant).</param>
    /// <param name="arquivo">PDF da CCT (multipart/form-data).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(OperacaoAceitaOutputDTO), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadCct(
        [FromRoute] Guid empresaId,
        IFormFile arquivo,
        CancellationToken cancellationToken)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new ArgumentException("Nenhum arquivo foi enviado ou o arquivo está vazio.", nameof(arquivo));

        if (!arquivo.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)
            && !arquivo.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Somente arquivos PDF são aceitos para upload de CCT.", nameof(arquivo));

        using var ms = new MemoryStream();
        await arquivo.CopyToAsync(ms, cancellationToken);
        var conteudo = ms.ToArray();

        var input = new UploadCctInputDTO(
            EmpresaId: empresaId,
            NomeArquivo: arquivo.FileName,
            ConteudoPdf: conteudo);

        await _processarCctUseCase.ExecutarAsync(input, cancellationToken);

        var processoId = Guid.NewGuid();
        return Accepted(new OperacaoAceitaOutputDTO(
            processoId,
            $"CCT '{arquivo.FileName}' indexada com sucesso. Regras disponíveis para auditoria."));
    }

    /// <summary>
    /// Gera o holerite narrativo em linguagem natural para um funcionário em um período,
    /// usando o Gemini 2.5 Flash com contexto das regras da CCT indexadas no Qdrant.
    /// </summary>
    /// <param name="empresaId">Identificador único da empresa (multi-tenant).</param>
    /// <param name="funcionarioId">Identificador único do funcionário.</param>
    /// <param name="periodoInicio">Início do período de apuração (yyyy-MM-dd).</param>
    /// <param name="periodoFim">Fim do período de apuração (yyyy-MM-dd).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    [HttpGet("holerite/{funcionarioId:guid}")]
    [ProducesResponseType(typeof(HoleriteNarrativoOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GerarHoleriteNarrativo(
        [FromRoute] Guid empresaId,
        [FromRoute] Guid funcionarioId,
        [FromQuery] DateOnly periodoInicio,
        [FromQuery] DateOnly periodoFim,
        CancellationToken cancellationToken)
    {
        if (periodoFim < periodoInicio)
            throw new ArgumentException("O período de fim não pode ser anterior ao início.", nameof(periodoFim));

        var resultado = await _holeriteUseCase.ExecutarAsync(
            empresaId,
            funcionarioId,
            periodoInicio,
            periodoFim,
            cancellationToken);
        return Ok(resultado);
    }
}
