using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entities;

namespace Infrastructure.AI;

/// <summary>
/// Helper interno de desserialização do JSON retornado pelo Gemini.
/// Separado do GeminiAuditorIaService para facilitar testes unitários.
/// </summary>
internal static class AnomaliaJsonParser
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Mapa de tipos: nomes retornados pelo Gemini → enums do domínio
    internal static readonly Dictionary<string, Domain.Enums.TipoAnomalia> MapaTipos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["AtrasoEntrada"]         = Domain.Enums.TipoAnomalia.BatidaForaDeSequencia,
            ["SaidaAntecipada"]       = Domain.Enums.TipoAnomalia.JornadaExcedida,
            ["IntervaloInsuficiente"] = Domain.Enums.TipoAnomalia.IntervaloInsuficiente,
            ["HoraExtraExcessiva"]    = Domain.Enums.TipoAnomalia.HoraExtraInesperada,
            ["FaltaInjustificada"]    = Domain.Enums.TipoAnomalia.FaltaDeRegistro,
            ["JornadaIncompleta"]     = Domain.Enums.TipoAnomalia.JornadaExcedida,
            ["RiscoDeDobra"]          = Domain.Enums.TipoAnomalia.RiscoDeDobra,
            ["BatidaForaDeSequencia"] = Domain.Enums.TipoAnomalia.BatidaForaDeSequencia,
        };

    public static IEnumerable<AlertaAnomalia> Parsear(string json, Funcionario funcionario)
    {
        List<AnomaliaJsonDto>? dtos;
        try
        {
            // Remove possível markdown residual (```json ... ```)
            var jsonLimpo = json
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            dtos = JsonSerializer.Deserialize<List<AnomaliaJsonDto>>(jsonLimpo, JsonOpts);
        }
        catch
        {
            return [];
        }

        if (dtos is null || dtos.Count == 0)
            return [];

        return dtos
            .Where(d => !string.IsNullOrWhiteSpace(d.TipoAnomalia))
            .Select(d =>
            {
                var tipo = MapaTipos.TryGetValue(d.TipoAnomalia!, out var t)
                    ? t
                    : Domain.Enums.TipoAnomalia.JornadaExcedida;

                return AlertaAnomalia.Criar(
                    empresaId: funcionario.EmpresaId,
                    funcionarioId: funcionario.Id,
                    tipoAnomalia: tipo,
                    dataReferencia: DateOnly.FromDateTime(d.DataOcorrencia),
                    descricao: d.Descricao ?? string.Empty,
                    gravidade: Math.Clamp(d.Gravidade, 1, 3));
            });
    }

    // DTO interno para desserialização do JSON retornado pelo Gemini
    internal sealed class AnomaliaJsonDto
    {
        [JsonPropertyName("tipoAnomalia")]
        public string? TipoAnomalia { get; set; }

        [JsonPropertyName("descricao")]
        public string? Descricao { get; set; }

        [JsonPropertyName("gravidade")]
        public int Gravidade { get; set; }

        [JsonPropertyName("dataOcorrencia")]
        public DateTime DataOcorrencia { get; set; }

        [JsonPropertyName("minutosAnomalia")]
        public int MinutosAnomalia { get; set; }
    }
}
