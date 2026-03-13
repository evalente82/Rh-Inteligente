using Application.DTOs;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Endpoints de autenticação — login e registro de usuários por tenant.
/// POST /api/{empresaId}/auth/login  — público (sem autenticação prévia)
/// POST /api/{empresaId}/auth/registrar — requer Dono ou Gestor
/// </summary>
[ApiController]
[Route("api/{empresaId:guid}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly RegistrarUsuarioUseCase _registrarUsuarioUseCase;

    public AuthController(LoginUseCase loginUseCase, RegistrarUsuarioUseCase registrarUsuarioUseCase)
    {
        _loginUseCase = loginUseCase;
        _registrarUsuarioUseCase = registrarUsuarioUseCase;
    }

    /// <summary>
    /// Autentica um usuário e retorna access token + refresh token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenOutputDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Login(
        [FromRoute] Guid empresaId,
        [FromBody] LoginInputDTO inputSemEmpresa,
        CancellationToken ct)
    {
        // Injeta o empresaId da rota no DTO
        var input = inputSemEmpresa with { EmpresaId = empresaId };
        var token = await _loginUseCase.ExecutarAsync(input, ct);
        return Ok(token);
    }

    /// <summary>
    /// Registra um novo usuário no tenant. Requer role Dono ou Gestor.
    /// </summary>
    [HttpPost("registrar")]
    [Authorize(Roles = "Dono,Gestor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RegistrarUsuario(
        [FromRoute] Guid empresaId,
        [FromBody] RegistrarUsuarioInputDTO inputSemEmpresa,
        CancellationToken ct)
    {
        var input = inputSemEmpresa with { EmpresaId = empresaId };
        var usuarioId = await _registrarUsuarioUseCase.ExecutarAsync(input, ct);
        return Created($"api/{empresaId}/auth/usuarios/{usuarioId}", new { usuarioId });
    }
}
