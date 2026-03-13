using Domain.Enums;
using Domain.ValueObjects;

namespace Domain.Entities;

/// <summary>
/// Representa um usuário do sistema vinculado a um tenant (empresa).
/// POCO puro — sem dependências externas.
/// </summary>
public sealed class Usuario
{
    public Guid Id { get; private set; }

    /// <summary>EmpresaId do tenant ao qual este usuário pertence (Regra 5 — multi-tenant).</summary>
    public Guid EmpresaId { get; private set; }

    public Email Email { get; private set; } = default!;

    /// <summary>Hash da senha (bcrypt). NUNCA armazenar texto plano.</summary>
    public string SenhaHash { get; private set; } = string.Empty;

    public string NomeCompleto { get; private set; } = string.Empty;

    public Role Role { get; private set; }

    public DateTime CriadoEm { get; private set; }

    public bool Ativo { get; private set; }

    /// <summary>Token de atualização opaco (refresh token).</summary>
    public string? RefreshToken { get; private set; }

    public DateTime? RefreshTokenExpiracao { get; private set; }

    private Usuario() { }

    /// <summary>
    /// Cria um novo usuário validando as invariantes do domínio.
    /// </summary>
    public static Usuario Criar(
        Guid empresaId,
        string emailString,
        string senhaHash,
        string nomeCompleto,
        Role role)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        ArgumentException.ThrowIfNullOrWhiteSpace(senhaHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeCompleto);

        var emailVo = Email.Criar(emailString);

        return new Usuario
        {
            Id = Guid.NewGuid(),
            EmpresaId = empresaId,
            Email = emailVo,
            SenhaHash = senhaHash,
            NomeCompleto = nomeCompleto.Trim(),
            Role = role,
            CriadoEm = DateTime.UtcNow,
            Ativo = true
        };
    }

    /// <summary>Atualiza o refresh token e sua expiração após novo login.</summary>
    public void DefinirRefreshToken(string refreshToken, DateTime expiracao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);
        if (expiracao <= DateTime.UtcNow)
            throw new ArgumentException("A expiração do refresh token deve ser futura.", nameof(expiracao));

        RefreshToken = refreshToken;
        RefreshTokenExpiracao = expiracao;
    }

    /// <summary>Invalida o refresh token (logout ou revogação).</summary>
    public void RevogarRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiracao = null;
    }

    /// <summary>Desativa o usuário impedindo novos logins.</summary>
    public void Desativar()
    {
        if (!Ativo)
            throw new InvalidOperationException("Usuário já está desativado.");

        Ativo = false;
        RevogarRefreshToken();
    }
}
