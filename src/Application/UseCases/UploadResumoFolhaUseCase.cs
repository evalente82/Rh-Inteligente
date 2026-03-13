using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Caso de Uso: recebe o arquivo de ponto enviado pelo gestor, persiste no storage
/// e enfileira a análise de IA em background.
///
/// Fluxo (Regra 3 do Roadmap — nunca bloquear a API):
///   1. Valida os dados de entrada.
///   2. Salva o arquivo no storage (ex: S3/Azure Blob) via IArmazenamentoArquivoService.
///   3. Enfileira o job de análise de IA via IAnalisadorBackgroundService.
///   4. Retorna imediatamente um OperacaoAceitaOutputDTO (HTTP 202).
///
/// Responsabilidade deste Use Case: APENAS aceitar e enfileirar.
/// Parsear o arquivo e auditar é responsabilidade do AnalisarRegistrosComIaUseCase.
/// </summary>
public sealed class UploadResumoFolhaUseCase
{
    private readonly IArmazenamentoArquivoService _armazenamento;
    private readonly IAnalisadorBackgroundService _analisadorBackground;

    public UploadResumoFolhaUseCase(
        IArmazenamentoArquivoService armazenamento,
        IAnalisadorBackgroundService analisadorBackground)
    {
        _armazenamento = armazenamento;
        _analisadorBackground = analisadorBackground;
    }

    /// <summary>
    /// Executa o use case. Retorna imediatamente com o ID do processo enfileirado.
    /// </summary>
    public async Task<OperacaoAceitaOutputDTO> ExecutarAsync(
        UploadResumoFolhaInputDTO input,
        CancellationToken cancellationToken = default)
    {
        ValidarInput(input);

        // Passo 1: persiste o arquivo no storage
        var caminhoArquivo = await _armazenamento.SalvarAsync(
            input.NomeArquivo,
            input.ConteudoArquivo,
            input.EmpresaId,
            cancellationToken);

        // Passo 2: enfileira o processamento para TODOS os funcionários do período.
        // O job concreto irá buscar os funcionários da empresa e disparar
        // um AnalisarRegistrosComIaUseCase para cada um em background.
        var processoId = await _analisadorBackground.EnfileirarAnaliseAsync(
            empresaId: input.EmpresaId,
            funcionarioId: Guid.Empty, // Guid.Empty = "todos da empresa neste período"
            periodoInicio: input.PeriodoInicio,
            periodoFim: input.PeriodoFim,
            cancellationToken: cancellationToken);

        return new OperacaoAceitaOutputDTO(
            ProcessoId: processoId,
            Mensagem: $"Arquivo '{input.NomeArquivo}' recebido. A auditoria de IA foi iniciada em segundo plano. Acompanhe pelo ID: {processoId}."
        );
    }

    // --- Validações de invariantes de entrada ---

    private static void ValidarInput(UploadResumoFolhaInputDTO input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.EmpresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(input));

        ArgumentException.ThrowIfNullOrWhiteSpace(input.NomeArquivo);

        if (input.ConteudoArquivo is null || input.ConteudoArquivo.Length == 0)
            throw new ArgumentException("O conteúdo do arquivo não pode ser vazio.", nameof(input));

        if (input.PeriodoFim < input.PeriodoInicio)
            throw new ArgumentException(
                "A data fim do período não pode ser anterior à data início.", nameof(input));
    }
}
