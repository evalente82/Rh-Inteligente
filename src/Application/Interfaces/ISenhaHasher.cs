namespace Application.Interfaces;

/// <summary>
/// Contrato para hashing e verificação de senhas.
/// Implementação concreta usa BCrypt na Infrastructure.
/// </summary>
public interface ISenhaHasher
{
    string Hashear(string senhaTextoPlano);
    bool Verificar(string senhaTextoPlano, string hash);
}
