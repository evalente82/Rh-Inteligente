namespace Application.Interfaces;

/// <summary>
/// Contrato para armazenamento e busca de vetores semânticos (embeddings) no banco vetorial.
/// Implementado na camada Infrastructure com Qdrant.
/// Cada vetor representa um chunk de texto de uma Convenção Coletiva (CCT).
/// </summary>
public interface IVectorRepository
{
    /// <summary>
    /// Garante que a coleção para uma empresa exista no Qdrant.
    /// Deve ser chamado antes do primeiro upsert de uma empresa.
    /// </summary>
    Task GarantirColecaoAsync(Guid empresaId, int dimensaoVetor, CancellationToken ct = default);

    /// <summary>
    /// Insere ou atualiza chunks vetorizados de uma CCT no banco vetorial.
    /// </summary>
    Task SalvarChunksAsync(
        Guid empresaId,
        IEnumerable<ChunkVetorizado> chunks,
        CancellationToken ct = default);

    /// <summary>
    /// Busca os N chunks mais similares ao vetor de consulta (RAG).
    /// </summary>
    Task<IEnumerable<ChunkVetorizado>> BuscarSimilaresAsync(
        Guid empresaId,
        float[] vetorConsulta,
        int topK = 5,
        CancellationToken ct = default);
}

/// <summary>
/// Representa um trecho (chunk) de texto de uma CCT já vetorizado.
/// Trafega entre Application e Infrastructure — sem dependência de frameworks.
/// </summary>
public sealed record ChunkVetorizado(
    Guid Id,
    Guid EmpresaId,
    string Texto,
    float[] Vetor,
    string FonteArquivo,
    int NumeroPagina);
