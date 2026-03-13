using Application.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Infrastructure.AI;

/// <summary>
/// Implementação de <see cref="ICctPdfParser"/> usando a biblioteca PdfPig.
/// Extrai texto página a página e divide em chunks de tamanho configurável,
/// respeitando quebras de parágrafo para não cortar frases no meio.
/// </summary>
internal sealed class CctPdfParserService : ICctPdfParser
{
    public IEnumerable<ChunkTexto> Extrair(
        byte[] conteudoPdf,
        string nomeArquivo,
        int tamanhoChunk = 1500)
    {
        if (conteudoPdf == null || conteudoPdf.Length == 0)
            yield break;

        using var documento = PdfDocument.Open(conteudoPdf);

        foreach (Page pagina in documento.GetPages())
        {
            var textoPagina = string.Join(" ", pagina.GetWords().Select(w => w.Text));

            if (string.IsNullOrWhiteSpace(textoPagina))
                continue;

            // Divide o texto da página em chunks sem cortar no meio de uma palavra
            foreach (var chunk in DividirEmChunks(textoPagina, tamanhoChunk))
            {
                if (!string.IsNullOrWhiteSpace(chunk))
                    yield return new ChunkTexto(
                        Texto: chunk.Trim(),
                        NomeArquivo: nomeArquivo,
                        NumeroPagina: pagina.Number);
            }
        }
    }

    private static IEnumerable<string> DividirEmChunks(string texto, int tamanho)
    {
        var inicio = 0;
        while (inicio < texto.Length)
        {
            var fim = Math.Min(inicio + tamanho, texto.Length);

            // Tenta recuar até um espaço para não cortar palavra
            if (fim < texto.Length)
            {
                var ultimoEspaco = texto.LastIndexOf(' ', fim, fim - inicio);
                if (ultimoEspaco > inicio)
                    fim = ultimoEspaco;
            }

            yield return texto[inicio..fim];
            inicio = fim;
        }
    }
}
