using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Processa o upload de uma Convenção Coletiva (CCT) em PDF:
/// 1. Extrai o texto via parser de PDF (PdfPig)
/// 2. Divide em chunks de ~1500 chars
/// 3. Gera embeddings via Gemini text-embedding-004
/// 4. Persiste os vetores no Qdrant (banco vetorial multi-tenant)
///
/// Retorna imediatamente com 202 Accepted (este use case é chamado dentro de um background job).
/// </summary>
public sealed class ProcessarCctUseCase
{
    private readonly ICctPdfParser _pdfParser;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepository;

    // Dimensão fixa do modelo text-embedding-004 do Google Gemini
    private const int DimensaoVetor = 768;

    public ProcessarCctUseCase(
        ICctPdfParser pdfParser,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepository)
    {
        _pdfParser = pdfParser;
        _embeddingService = embeddingService;
        _vectorRepository = vectorRepository;
    }

    public async Task ExecutarAsync(UploadCctInputDTO input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.ConteudoPdf.Length == 0)
            throw new ArgumentException("O arquivo PDF não pode estar vazio.", nameof(input));

        // 1. Garante que a coleção do tenant existe no Qdrant
        await _vectorRepository.GarantirColecaoAsync(input.EmpresaId, DimensaoVetor, ct);

        // 2. Extrai chunks de texto do PDF
        var chunks = _pdfParser.Extrair(input.ConteudoPdf, input.NomeArquivo).ToList();

        if (chunks.Count == 0)
            throw new InvalidOperationException("Nenhum texto extraído do PDF. Verifique se o arquivo é uma CCT válida.");

        // 3. Gera embeddings em lote (uma chamada à API por batida de 100 chunks)
        var textos = chunks.Select(c => c.Texto).ToList();
        var vetores = (await _embeddingService.GerarEmbeddingLoteAsync(textos, ct)).ToList();

        // 4. Monta os chunks vetorizados e persiste no Qdrant
        var chunksVetorizados = chunks.Zip(vetores, (chunk, vetor) =>
            new ChunkVetorizado(
                Id: Guid.NewGuid(),
                EmpresaId: input.EmpresaId,
                Texto: chunk.Texto,
                Vetor: vetor,
                FonteArquivo: chunk.NomeArquivo,
                NumeroPagina: chunk.NumeroPagina));

        await _vectorRepository.SalvarChunksAsync(input.EmpresaId, chunksVetorizados, ct);
    }
}
