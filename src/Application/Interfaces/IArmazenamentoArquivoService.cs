namespace Application.Interfaces;

/// <summary>
/// Contrato para armazenamento de arquivos enviados pelo usuário (ex: CSV/XLSX de ponto).
/// Implementado na camada Infrastructure (Azure Blob, S3 ou disco local).
/// </summary>
public interface IArmazenamentoArquivoService
{
    /// <summary>
    /// Persiste o arquivo e retorna o caminho/URI de acesso.
    /// </summary>
    /// <param name="nomeArquivo">Nome original do arquivo.</param>
    /// <param name="conteudo">Stream com o conteúdo binário.</param>
    /// <param name="empresaId">Tenant owner do arquivo.</param>
    Task<string> SalvarAsync(
        string nomeArquivo,
        Stream conteudo,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o conteúdo de um arquivo previamente armazenado.
    /// </summary>
    Task<Stream> ObterAsync(string caminhoArquivo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove um arquivo do armazenamento.
    /// </summary>
    Task RemoverAsync(string caminhoArquivo, CancellationToken cancellationToken = default);
}
