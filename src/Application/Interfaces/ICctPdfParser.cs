namespace Application.Interfaces;

/// <summary>
/// Contrato para parsing de arquivos PDF de Convenção Coletiva (CCT).
/// Extrai texto e divide em chunks de tamanho adequado para embeddings.
/// Implementado na camada Infrastructure com PdfPig.
/// </summary>
public interface ICctPdfParser
{
    /// <summary>
    /// Extrai o texto do PDF e retorna os chunks prontos para vetorização.
    /// </summary>
    /// <param name="conteudoPdf">Bytes brutos do arquivo PDF.</param>
    /// <param name="nomeArquivo">Nome original do arquivo (para rastreabilidade).</param>
    /// <param name="tamanhoChunk">Tamanho máximo de cada chunk em caracteres (default: 1500).</param>
    IEnumerable<ChunkTexto> Extrair(
        byte[] conteudoPdf,
        string nomeArquivo,
        int tamanhoChunk = 1500);
}

/// <summary>
/// Representa um trecho de texto extraído de um PDF antes da vetorização.
/// </summary>
public sealed record ChunkTexto(
    string Texto,
    string NomeArquivo,
    int NumeroPagina);
