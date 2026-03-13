using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace Infrastructure.AI;

/// <summary>
/// Implementação de <see cref="IAuditorIaService"/> usando o modelo gemini-2.5-flash.
/// Fluxo RAG:
///   1. Gera embedding da pergunta de auditoria
///   2. Busca os top-5 chunks de CCT mais relevantes no Qdrant
///   3. Monta o System Prompt com os chunks como contexto
///   4. Envia ao Gemini e parseia o JSON estruturado de resposta
/// </summary>
internal sealed class GeminiAuditorIaService : IAuditorIaService
{
    private readonly GoogleAI _gemini;
    private readonly GeminiOptions _options;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;

    // Prompt de sistema imutável — define comportamento estrito do modelo
    private const string SystemPrompt = """
        Você é um auditor trabalhista especializado em legislação brasileira (CLT) e Convenções Coletivas de Trabalho (CCT).
        Sua única função é analisar os registros de ponto fornecidos e identificar anomalias, infrações ou riscos trabalhistas.

        REGRAS ABSOLUTAS:
        1. Responda EXCLUSIVAMENTE em JSON válido, sem markdown, sem texto antes ou depois do JSON.
        2. O JSON deve ser um array de objetos. Se não houver anomalias, retorne um array vazio: []
        3. Cada objeto do array deve ter EXATAMENTE estas propriedades:
           - "tipoAnomalia": string (valores permitidos: "AtrasoEntrada", "SaidaAntecipada", "IntervaloInsuficiente", "HoraExtraExcessiva", "FaltaInjustificada", "JornadaIncompleta")
           - "descricao": string (máximo 200 caracteres, em português)
           - "gravidade": integer (1=Informativo, 2=Atenção, 3=Crítico)
           - "dataOcorrencia": string (formato ISO 8601: "YYYY-MM-DDTHH:mm:ss")
           - "minutosAnomalia": integer (duração da anomalia em minutos, 0 se não aplicável)
        4. NÃO invente dados. Use apenas as informações fornecidas nos registros.
        5. Considere o turno contratual do funcionário ao analisar os registros.
        6. Use os trechos da CCT fornecidos para embasar os alertas críticos.
        """;

    public GeminiAuditorIaService(
        IOptions<GeminiOptions> options,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository)
    {
        _options = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.ApiKey, nameof(_options.ApiKey));
        _gemini = new GoogleAI(_options.ApiKey);
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
    }

    public async Task<IEnumerable<AlertaAnomalia>> AnalisarAsync(
        Funcionario funcionario,
        IEnumerable<RegistroPonto> registros,
        CancellationToken cancellationToken = default)
    {
        var listaRegistros = registros.ToList();

        if (listaRegistros.Count == 0)
            return [];

        // ── 1. RAG: busca regras CCT relevantes no Qdrant ────────────────────
        var contextoCct = await BuscarContextoCctAsync(
            funcionario.EmpresaId,
            $"jornada de trabalho horas extras intervalo {funcionario.TurnoContratual.CargaHorariaDiaria.TotalHours}h",
            cancellationToken);

        // ── 2. Monta o prompt com os dados do funcionário + registros + CCT ──
        var promptUsuario = MontarPromptAuditoria(funcionario, listaRegistros, contextoCct);

        // ── 3. Chama o Gemini 2.5 Flash ──────────────────────────────────────
        var modelo = _gemini.GenerativeModel(
            model: _options.ModeloChat,
            systemInstruction: new Content(SystemPrompt));

        var resposta = await modelo.GenerateContent(promptUsuario);
        var textoResposta = resposta.Text?.Trim() ?? "[]";

        // ── 4. Desserializa o JSON e converte para entidades de domínio ───────
        return DesserializarAlertas(textoResposta, funcionario);
    }

    private async Task<string> BuscarContextoCctAsync(
        Guid empresaId,
        string consulta,
        CancellationToken ct)
    {
        try
        {
            var vetorConsulta = await _embeddingService.GerarEmbeddingAsync(consulta, ct);
            var chunks = await _vectorRepository.BuscarSimilaresAsync(empresaId, vetorConsulta, topK: 5, ct);
            var textos = chunks.Select(c => $"[Página {c.NumeroPagina}]: {c.Texto}").ToList();

            return textos.Count > 0
                ? "TRECHOS RELEVANTES DA CONVENÇÃO COLETIVA:\n" + string.Join("\n\n", textos)
                : "Nenhuma CCT cadastrada para esta empresa. Use apenas a CLT como base.";
        }
        catch
        {
            // Se o Qdrant não estiver disponível, continua sem o contexto de CCT
            return "Contexto CCT indisponível. Use apenas a CLT como base.";
        }
    }

    private static string MontarPromptAuditoria(
        Funcionario funcionario,
        List<RegistroPonto> registros,
        string contextoCct)
    {
        var sb = new StringBuilder();
        sb.AppendLine(contextoCct);
        sb.AppendLine();
        sb.AppendLine("DADOS DO FUNCIONÁRIO:");
        sb.AppendLine($"- Nome: {funcionario.Nome}");
        sb.AppendLine($"- Turno contratual: {funcionario.TurnoContratual.HoraEntrada:HH\\:mm} às {funcionario.TurnoContratual.HoraSaida:HH\\:mm}");
        sb.AppendLine($"- Intervalo contratual: {funcionario.TurnoContratual.DuracaoIntervalo.TotalMinutes} minutos");
        sb.AppendLine($"- Carga horária diária: {funcionario.TurnoContratual.CargaHorariaDiaria.TotalHours:F1}h");
        sb.AppendLine();
        sb.AppendLine("REGISTROS DE PONTO:");

        foreach (var grupo in registros.GroupBy(r => r.DataHoraBatida.Date).OrderBy(g => g.Key))
        {
            sb.AppendLine($"  Data: {grupo.Key:dd/MM/yyyy}");
            foreach (var r in grupo.OrderBy(r => r.DataHoraBatida))
                sb.AppendLine($"    - {r.DataHoraBatida:HH:mm} | {r.TipoBatida} {(r.LancamentoManual ? "(manual)" : "")}");
        }

        sb.AppendLine();
        sb.AppendLine("Analise os registros e retorne o JSON de anomalias conforme instruído.");
        return sb.ToString();
    }

    private static IEnumerable<AlertaAnomalia> DesserializarAlertas(
        string json,
        Funcionario funcionario)
        => AnomaliaJsonParser.Parsear(json, funcionario);

    // Mapa de tipos e DTO ficaram em AnomaliaJsonParser.cs (reutilizável em testes)
}
