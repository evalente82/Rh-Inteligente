using Application.Interfaces;
using BC = BCrypt.Net.BCrypt;

namespace Infrastructure.Auth;

/// <summary>
/// Implementação de hashing de senhas usando BCrypt (work factor 12).
/// </summary>
public sealed class BcryptSenhaHasher : ISenhaHasher
{
    private const int WorkFactor = 12;

    public string Hashear(string senhaTextoPlano)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaTextoPlano);
        return BC.HashPassword(senhaTextoPlano, WorkFactor);
    }

    public bool Verificar(string senhaTextoPlano, string hash)
    {
        if (string.IsNullOrWhiteSpace(senhaTextoPlano)) return false;
        if (string.IsNullOrWhiteSpace(hash)) return false;
        return BC.Verify(senhaTextoPlano, hash);
    }
}
