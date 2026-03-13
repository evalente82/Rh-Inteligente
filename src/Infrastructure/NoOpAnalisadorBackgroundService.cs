using Application.Interfaces;

namespace Infrastructure;

/// <summary>
/// Implementação NoOp de <see cref="IAnalisadorBackgroundService"/>.
/// Em produção, substitua por Hangfire / RabbitMQ / Azure Service Bus (Regra 3).
/// </summary>
internal sealed class NoOpAnalisadorBackgroundService : IAnalisadorBackgroundService
{
    public Task<Guid> EnfileirarAnaliseAsync(
        Guid empresaId,
        Guid funcionarioId,
        DateOnly periodoInicio,
        DateOnly periodoFim,
        CancellationToken cancellationToken = default)
    {
        // Stub: retorna um processId sem executar nada
        return Task.FromResult(Guid.NewGuid());
    }
}
