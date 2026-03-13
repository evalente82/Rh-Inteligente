namespace Infrastructure;

/// <summary>
/// Abstração para resolução do EmpresaId (tenant ativo) no escopo da requisição HTTP.
/// Implementado na camada Infrastructure via HttpContext — mantido fora do Domain/Application
/// para não violar a Clean Architecture.
/// </summary>
public interface ITenantProvider
{
    /// <summary>
    /// Identificador da empresa (tenant) resolvido a partir da rota HTTP atual.
    /// Deve ser Guid.Empty em contextos fora do ciclo de requisição (ex: jobs).
    /// </summary>
    Guid EmpresaId { get; }
}
