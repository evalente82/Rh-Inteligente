namespace Infrastructure.AI;

/// <summary>
/// Configurações do Qdrant lidas do appsettings.json.
/// Seção: "Qdrant"
/// </summary>
public sealed class QdrantOptions
{
    public const string SecaoConfig = "Qdrant";

    /// <summary>Host do Qdrant (default: localhost).</summary>
    public string Host { get; set; } = "localhost";

    /// <summary>Porta gRPC do Qdrant (default: 6334).</summary>
    public int Porta { get; set; } = 6334;

    /// <summary>
    /// Prefixo de ambiente para isolar coleções no mesmo cluster Qdrant.
    /// Dev  → "dev"  → coleção: "dev_cct_{empresaId}"
    /// Prod → "prod" → coleção: "prod_cct_{empresaId}"
    /// Garante que um único container Qdrant sirva múltiplos ambientes sem colisão.
    /// </summary>
    public string AmbientePrefix { get; set; } = "dev";
}
