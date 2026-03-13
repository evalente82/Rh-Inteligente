namespace Application.Interfaces;

/// <summary>
/// Contrato para geração de embeddings de texto via LLM.
/// Implementado na camada Infrastructure com a API Gemini text-embedding-004.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Gera o vetor de embedding para um único texto.
    /// Retorna um array de floats com a dimensão do modelo (text-embedding-004 = 768 dims).
    /// </summary>
    Task<float[]> GerarEmbeddingAsync(string texto, CancellationToken ct = default);

    /// <summary>
    /// Gera embeddings em lote. Mais eficiente para múltiplos chunks de um PDF.
    /// </summary>
    Task<IEnumerable<float[]>> GerarEmbeddingLoteAsync(
        IEnumerable<string> textos,
        CancellationToken ct = default);
}
