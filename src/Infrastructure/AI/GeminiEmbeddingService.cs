using Application.Interfaces;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;

namespace Infrastructure.AI;

/// <summary>
/// Implementação de <see cref="IEmbeddingService"/> usando o modelo
/// text-embedding-004 do Google Gemini via Mscc.GenerativeAI.
/// Dimensão do vetor de saída: 768 floats.
/// </summary>
internal sealed class GeminiEmbeddingService : IEmbeddingService
{
    // Modelo de embeddings do Gemini — menor custo, 768 dimensões
    private const string ModeloEmbedding = "text-embedding-004";
    // Lote máximo por chamada à API (limite do Gemini batchEmbedContents)
    private const int TamanhoLote = 100;

    private readonly GoogleAI _gemini;

    public GeminiEmbeddingService(IOptions<GeminiOptions> options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.ApiKey, nameof(options.Value.ApiKey));
        _gemini = new GoogleAI(options.Value.ApiKey);
    }

    public async Task<float[]> GerarEmbeddingAsync(string texto, CancellationToken ct = default)
    {
        var resultados = await GerarEmbeddingLoteAsync([texto], ct);
        return resultados.First();
    }

    public async Task<IEnumerable<float[]>> GerarEmbeddingLoteAsync(
        IEnumerable<string> textos,
        CancellationToken ct = default)
    {
        var listaTextos = textos.ToList();
        var vetores = new List<float[]>(listaTextos.Count);

        // Processa em lotes de TamanhoLote para respeitar o limite da API
        foreach (var lote in listaTextos.Chunk(TamanhoLote))
        {
            ct.ThrowIfCancellationRequested();

            var modelo = _gemini.GenerativeModel(ModeloEmbedding);
            // EmbedContent(List<EmbedContentRequest>) — batch nativo do SDK 1.9.x
            var requests = lote.Select(t => new EmbedContentRequest(t)).ToList();
            var resposta = await modelo.EmbedContent(requests);

            // Batch retorna .Embeddings (plural); cada item é ContentEmbedding com .Values
            foreach (var embedding in resposta.Embeddings!)
            {
                vetores.Add(embedding.Values?.Select(v => (float)v).ToArray()
                    ?? throw new InvalidOperationException(
                        "Gemini retornou embedding nulo. Verifique a API Key e o modelo configurado."));
            }
        }

        return vetores;
    }
}
