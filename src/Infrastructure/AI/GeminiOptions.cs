namespace Infrastructure.AI;

/// <summary>
/// Configurações do Google Gemini lidas do appsettings.json.
/// Seção: "Gemini"
/// </summary>
public sealed class GeminiOptions
{
    public const string SecaoConfig = "Gemini";

    /// <summary>API Key do Google AI Studio.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Modelo de geração de texto (default: gemini-2.5-flash).</summary>
    public string ModeloChat { get; set; } = "gemini-2.5-flash";
}
