using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Autentica um usuário e retorna access token + refresh token.
/// </summary>
public sealed class LoginUseCase
{
    // Validade do refresh token em dias
    private const int DiasRefreshToken = 7;

    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;

    public LoginUseCase(
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher,
        IJwtService jwtService,
        IUnitOfWork unitOfWork)
    {
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
    }

    public async Task<TokenOutputDTO> ExecutarAsync(LoginInputDTO input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var usuario = await _usuarioRepository.ObterPorEmailAsync(input.EmpresaId, input.Email, ct)
            ?? throw new KeyNotFoundException("E-mail ou senha inválidos.");

        if (!usuario.Ativo)
            throw new InvalidOperationException("Usuário desativado. Contate o administrador.");

        if (!_senhaHasher.Verificar(input.Senha, usuario.SenhaHash))
            throw new KeyNotFoundException("E-mail ou senha inválidos.");

        var accessToken  = _jwtService.GerarAccessToken(usuario);
        var refreshToken = _jwtService.GerarRefreshToken();
        var expiracao    = DateTime.UtcNow.AddDays(DiasRefreshToken);

        usuario.DefinirRefreshToken(refreshToken, expiracao);
        _usuarioRepository.Atualizar(usuario);
        await _unitOfWork.CommitAsync(ct);

        // Validade do access token: 1 hora (controlada pelo JwtService)
        return new TokenOutputDTO(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            usuario.Id,
            usuario.EmpresaId,
            usuario.NomeCompleto,
            usuario.Role.ToString());
    }
}
