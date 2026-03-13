namespace Application.Interfaces;

/// <summary>
/// Contrato para enfileirar um job de análise de IA em background.
/// A implementação concreta pode usar Hangfire, RabbitMQ, Azure Service Bus, etc.
/// (Regra 3 do Roadmap — processamento assíncrono obrigatório).
/// </summary>
public interface IAnalisadorBackgroundService
{
    /// <summary>
    /// Enfileira a análise de IA para o funcionário no período informado.
    /// Retorna o identificador do processo para rastreamento.
    /// </summary>
    Task<Guid> EnfileirarAnaliseAsync(
        Guid empresaId,
        Guid funcionarioId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken cancellationToken = default);
}
