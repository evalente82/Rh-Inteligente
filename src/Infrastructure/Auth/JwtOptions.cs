namespace Infrastructure.Auth;

/// <summary>
/// Opções de configuração do JWT lidas do appsettings.
/// </summary>
public sealed class JwtOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "VcorpRhInteligente";
    public string Audience { get; set; } = "VcorpRhInteligenteUsers";

    /// <summary>Validade do access token em minutos. Padrão: 60.</summary>
    public int ExpiracaoMinutos { get; set; } = 60;
}
