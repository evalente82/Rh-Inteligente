using Application.Interfaces;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace Infrastructure.AI;

/// <summary>
/// Implementação de <see cref="IVectorRepository"/> usando o Qdrant via gRPC.
/// Cada empresa tem sua própria coleção isolada: "cct_{empresaId}".
/// </summary>
internal sealed class QdrantVectorRepository : IVectorRepository
{
    private readonly QdrantClient _client;
    private readonly string _ambientePrefix;

    public QdrantVectorRepository(IOptions<QdrantOptions> options)
    {
        _client = new QdrantClient(options.Value.Host, options.Value.Porta);
        _ambientePrefix = options.Value.AmbientePrefix.ToLowerInvariant();
    }

    // Formato: "{env}_cct_{empresaId:N}"
    // Dev  → "dev_cct_11111111000000000000000000000001"
    // Prod → "prod_cct_11111111000000000000000000000001"
    private string NomeColecao(Guid empresaId) => $"{_ambientePrefix}_cct_{empresaId:N}";

    public async Task GarantirColecaoAsync(Guid empresaId, int dimensaoVetor, CancellationToken ct = default)
    {
        var nomeColecao = NomeColecao(empresaId);
        var colecoes = await _client.ListCollectionsAsync(ct);

        if (!colecoes.Any(c => c == nomeColecao))
        {
            await _client.CreateCollectionAsync(
                nomeColecao,
                new VectorParams
                {
                    Size = (ulong)dimensaoVetor,
                    Distance = Distance.Cosine
                },
                cancellationToken: ct);
        }
    }

    public async Task SalvarChunksAsync(
        Guid empresaId,
        IEnumerable<ChunkVetorizado> chunks,
        CancellationToken ct = default)
    {
        var nomeColecao = NomeColecao(empresaId);
        var pontos = chunks.Select(c => new PointStruct
        {
            Id = new PointId { Uuid = c.Id.ToString() },
            Vectors = c.Vetor,
            Payload =
            {
                ["texto"] = c.Texto,
                ["fonte"] = c.FonteArquivo,
                ["pagina"] = c.NumeroPagina,
                ["empresa_id"] = c.EmpresaId.ToString()
            }
        }).ToList();

        await _client.UpsertAsync(nomeColecao, pontos, cancellationToken: ct);
    }

    public async Task<IEnumerable<ChunkVetorizado>> BuscarSimilaresAsync(
        Guid empresaId,
        float[] vetorConsulta,
        int topK = 5,
        CancellationToken ct = default)
    {
        var nomeColecao = NomeColecao(empresaId);

        var resultados = await _client.SearchAsync(
            nomeColecao,
            vetorConsulta,
            limit: (ulong)topK,
            cancellationToken: ct);

        return resultados.Select(r => new ChunkVetorizado(
            Id: Guid.Parse(r.Id.Uuid),
            EmpresaId: empresaId,
            Texto: r.Payload.TryGetValue("texto", out var t) ? t.StringValue : string.Empty,
            Vetor: [],
            FonteArquivo: r.Payload.TryGetValue("fonte", out var f) ? f.StringValue : string.Empty,
            NumeroPagina: r.Payload.TryGetValue("pagina", out var p) ? (int)p.IntegerValue : 0));
    }
}
