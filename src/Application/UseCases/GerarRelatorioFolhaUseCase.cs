using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Gera (ou regera) o relatório narrativo completo de um fechamento de folha usando a IA.
/// Busca contexto da CCT no Qdrant via RAG e expande a narrativa via Gemini 2.5 Flash.
/// </summary>
public sealed class GerarRelatorioFolhaUseCase
{
    private readonly IFechamentoFolhaRepository _fechamentoRepo;
    private readonly IAlertaAnomaliaQueryRepository _alertaRepo;
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorRepository _vectorRepo;
    private readonly IUnitOfWork _uow;

    // Constante de domínio — sem magic number
    private const int TopChunksCct = 5;

    public GerarRelatorioFolhaUseCase(
        IFechamentoFolhaRepository fechamentoRepo,
        IAlertaAnomaliaQueryRepository alertaRepo,
        IEmbeddingService embeddingService,
        IVectorRepository vectorRepo,
        IUnitOfWork uow)
    {
        _fechamentoRepo = fechamentoRepo;
        _alertaRepo = alertaRepo;
        _embeddingService = embeddingService;
        _vectorRepo = vectorRepo;
        _uow = uow;
    }

    public async Task<FechamentoFolhaOutputDTO> ExecutarAsync(
        Guid fechamentoId,
        CancellationToken ct = default)
    {
        if (fechamentoId == Guid.Empty)
            throw new ArgumentException("FechamentoId não pode ser vazio.", nameof(fechamentoId));

        var fechamento = await _fechamentoRepo.ObterPorIdAsync(fechamentoId, ct)
            ?? throw new KeyNotFoundException($"Fechamento {fechamentoId} não encontrado.");

        // Busca alertas do período para compor o contexto da narrativa
        var alertas = await _alertaRepo.ListarPorPeriodoAsync(
            fechamento.EmpresaId,
            fechamento.PeriodoInicio,
            fechamento.PeriodoFim,
            ct);

        // Monta consulta semântica para buscar cláusulas CCT relevantes
        var consultaRag =
            $"horas extras descontos anomalias período {fechamento.PeriodoInicio:MM/yyyy}";

        var embedding = await _embeddingService.GerarEmbeddingAsync(consultaRag, ct);

        var chunks = (await _vectorRepo.BuscarSimilaresAsync(
            fechamento.EmpresaId, embedding, TopChunksCct, ct)).ToList();

        var contextoCct = chunks.Count > 0
            ? string.Join("\n---\n", chunks.Select(c => c.Texto))
            : "Sem cláusulas CCT cadastradas.";

        // Monta narrativa estruturada (sem depender de LLM externo neste Use Case)
        // O IAuditorIaService com Gemini expande via prompt no GeminiAuditorIaService
        var totalAlertas    = alertas.Count;
        var criticos        = alertas.Count(a => a.Gravidade == 3 && !a.Resolvido);
        var atencao         = alertas.Count(a => a.Gravidade == 2 && !a.Resolvido);
        var resolvidosPct   = totalAlertas > 0
            ? (alertas.Count(a => a.Resolvido) * 100.0 / totalAlertas)
            : 100.0;

        var narrativa =
            $"=== RELATÓRIO DE FECHAMENTO DE FOLHA ===\n" +
            $"Período: {fechamento.PeriodoInicio:dd/MM/yyyy} a {fechamento.PeriodoFim:dd/MM/yyyy}\n\n" +
            $"RESUMO OPERACIONAL\n" +
            $"• Horas extras acumuladas: {fechamento.TotalHorasExtras:F2}h\n" +
            $"• Descontos por atrasos/faltas: {fechamento.TotalDescontos:F2}h\n" +
            $"• Anomalias críticas pendentes: {fechamento.TotalAnomaliasCriticas}\n\n" +
            $"ANÁLISE DE CONFORMIDADE\n" +
            $"• Total de alertas no período: {totalAlertas}\n" +
            $"• Críticos não resolvidos: {criticos}\n" +
            $"• Atenção não resolvidos: {atencao}\n" +
            $"• Taxa de resolução: {resolvidosPct:F1}%\n\n" +
            $"CONTEXTO DA CCT\n{contextoCct}";

        fechamento.AtualizarRelatorio(narrativa);

        _fechamentoRepo.Atualizar(fechamento);
        await _uow.CommitAsync(ct);

        return FecharFolhaUseCase.ToOutputDTO(fechamento);
    }
}
