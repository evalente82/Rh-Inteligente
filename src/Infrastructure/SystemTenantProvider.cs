namespace Infrastructure;

/// <summary>
/// ITenantProvider para uso em contextos de sistema (jobs, seed, migrations).
/// Retorna Guid.Empty, desabilitando os Global Query Filters de tenant.
/// NÃO deve ser registrado como implementação padrão no DI — apenas usado
/// explicitamente na criação do DataSeeder.
/// </summary>
internal sealed class SystemTenantProvider : ITenantProvider
{
    /// <summary>Guid.Empty desativa o Global Query Filter em modo sistema.</summary>
    public Guid EmpresaId => Guid.Empty;
}
