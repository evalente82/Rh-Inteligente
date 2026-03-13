using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato para geração e validação de tokens JWT.
/// Implementação concreta na Infrastructure não deve vazar para o Domain/Application.
/// </summary>
public interface IJwtService
{
    /// <summary>Gera um access token JWT com claims do usuário.</summary>
    string GerarAccessToken(Usuario usuario);

    /// <summary>Gera um refresh token opaco (string aleatória segura).</summary>
    string GerarRefreshToken();
}
