using Application.DTOs;
using Application.Interfaces;

namespace Infrastructure;

/// <summary>
/// Implementação local (dev) de <see cref="IArmazenamentoArquivoService"/>.
/// Salva na pasta temp do sistema. Em produção, substitua por Azure Blob / S3.
/// </summary>
internal sealed class LocalArmazenamentoArquivoService : IArmazenamentoArquivoService
{
    public async Task<string> SalvarAsync(
        string nomeArquivo,
        Stream conteudo,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var pasta = Path.Combine(Path.GetTempPath(), "vcorp_uploads", empresaId.ToString("N"));
        Directory.CreateDirectory(pasta);

        var caminho = Path.Combine(pasta, $"{Guid.NewGuid():N}_{nomeArquivo}");
        await using var fs = File.Create(caminho);
        await conteudo.CopyToAsync(fs, cancellationToken);

        return caminho;
    }

    public Task<Stream> ObterAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        Stream fs = File.OpenRead(caminhoArquivo);
        return Task.FromResult(fs);
    }

    public Task RemoverAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        if (File.Exists(caminhoArquivo))
            File.Delete(caminhoArquivo);
        return Task.CompletedTask;
    }
}
