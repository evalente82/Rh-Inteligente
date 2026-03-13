using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Endpoints de onboarding de empresas (tenants) no SaaS.
/// POST /api/empresas — rota pública (sem autenticação) para criar novo tenant.
/// </summary>
[ApiController]
[Route("api/empresas")]
public sealed class EmpresaController : ControllerBase
{
    private readonly CriarEmpresaUseCase _criarEmpresaUseCase;

    public EmpresaController(CriarEmpresaUseCase criarEmpresaUseCase)
    {
        _criarEmpresaUseCase = criarEmpresaUseCase;
    }

    /// <summary>
    /// Cria uma nova empresa (tenant) e o usuário Dono atomicamente.
    /// Retorna os dados da empresa criada com status 201.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmpresaOutputDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CriarEmpresa(
        [FromBody] CriarEmpresaInputDTO input,
        CancellationToken ct)
    {
        var resultado = await _criarEmpresaUseCase.ExecutarAsync(input, ct);
        return CreatedAtAction(nameof(CriarEmpresa), new { id = resultado.Id }, resultado);
    }
}
