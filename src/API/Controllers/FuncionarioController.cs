using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller responsável pelo ciclo de vida do Funcionário: cadastro, admissão, listagem e demissão.
/// Rota base: /api/{empresaId}/funcionarios
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/funcionarios")]
[Produces("application/json")]
[Authorize(Roles = "Dono,Gestor")]
public sealed class FuncionarioController : ControllerBase
{
    private readonly CadastrarFuncionarioUseCase _cadastrarUseCase;
    private readonly AdmitirFuncionarioUseCase _admitirUseCase;
    private readonly ListarFuncionariosUseCase _listarUseCase;
    private readonly DemitirFuncionarioUseCase _demitirUseCase;

    public FuncionarioController(
        CadastrarFuncionarioUseCase cadastrarUseCase,
        AdmitirFuncionarioUseCase admitirUseCase,
        ListarFuncionariosUseCase listarUseCase,
        DemitirFuncionarioUseCase demitirUseCase)
    {
        _cadastrarUseCase = cadastrarUseCase;
        _admitirUseCase = admitirUseCase;
        _listarUseCase = listarUseCase;
        _demitirUseCase = demitirUseCase;
    }

    /// <summary>
    /// Cadastra um novo funcionário (pré-admissão).
    /// A admissão formal (cargo, salário, regime, endereço) é feita em POST /{id}/admissao.
    /// </summary>
    /// <response code="201">Funcionário cadastrado com sucesso.</response>
    /// <response code="409">Matrícula ou CPF já cadastrados nesta empresa.</response>
    [HttpPost]
    [ProducesResponseType(typeof(FuncionarioOutputDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cadastrar(
        Guid empresaId,
        [FromBody] CadastrarFuncionarioInputDTO input,
        CancellationToken cancellationToken)
    {
        var dto = input with { EmpresaId = empresaId };
        var resultado = await _cadastrarUseCase.ExecutarAsync(dto, cancellationToken);
        return CreatedAtAction(
            nameof(ObterPorId),
            new { empresaId, id = resultado.Id },
            resultado);
    }

    /// <summary>
    /// Lista todos os funcionários ativos da empresa.
    /// </summary>
    /// <response code="200">Lista de funcionários retornada.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<FuncionarioOutputDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        Guid empresaId,
        CancellationToken cancellationToken)
    {
        var resultado = await _listarUseCase.ExecutarAsync(empresaId, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Retorna os dados de um funcionário específico pelo ID.
    /// </summary>
    /// <response code="200">Funcionário encontrado.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FuncionarioOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(
        Guid empresaId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var resultado = await _listarUseCase.ExecutarPorIdAsync(id, empresaId, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Formaliza a admissão de um funcionário: cargo, salário, regime contratual e endereço residencial.
    /// </summary>
    /// <response code="200">Admissão registrada com sucesso.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    /// <response code="409">Funcionário já possui admissão ativa.</response>
    [HttpPost("{id:guid}/admissao")]
    [ProducesResponseType(typeof(FuncionarioOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Admitir(
        Guid empresaId,
        Guid id,
        [FromBody] AdmitirFuncionarioInputDTO input,
        CancellationToken cancellationToken)
    {
        var dto = input with { EmpresaId = empresaId, FuncionarioId = id };
        var resultado = await _admitirUseCase.ExecutarAsync(dto, cancellationToken);
        return Ok(resultado);
    }

    /// <summary>
    /// Realiza o desligamento formal do funcionário.
    /// </summary>
    /// <response code="204">Funcionário desligado com sucesso.</response>
    /// <response code="404">Funcionário não encontrado.</response>
    /// <response code="409">Funcionário já está demitido.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Demitir(
        Guid empresaId,
        Guid id,
        [FromQuery] DateTime dataDemissao,
        CancellationToken cancellationToken)
    {
        await _demitirUseCase.ExecutarAsync(empresaId, id, dataDemissao, cancellationToken);
        return NoContent();
    }
}
