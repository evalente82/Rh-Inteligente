using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence;

/// <summary>
/// Factory usada exclusivamente pelo EF Core Tools (dotnet ef migrations add, database update).
/// Necessária porque o RhInteligenteDbContext recebe ITenantProvider via construtor (multi-tenant),
/// o que impede o EF de instanciá-lo automaticamente pelo DI em design-time.
/// NÃO é registrada no DI nem usada em runtime.
/// </summary>
internal sealed class RhInteligenteDbContextDesignTimeFactory
    : IDesignTimeDbContextFactory<RhInteligenteDbContext>
{
    public RhInteligenteDbContext CreateDbContext(string[] args)
    {
        // Lê a connection string do appsettings do projeto API em design-time
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "API"))
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' não encontrada no appsettings.Development.json da API.");

        var optionsBuilder = new DbContextOptionsBuilder<RhInteligenteDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
            npgsql.MigrationsAssembly(typeof(RhInteligenteDbContext).Assembly.FullName));

        // Em design-time o EmpresaId não importa — usa um stub com Guid.Empty
        return new RhInteligenteDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }

    /// <summary>Stub de ITenantProvider para uso exclusivo em design-time (migrations).</summary>
    private sealed class DesignTimeTenantProvider : ITenantProvider
    {
        public Guid EmpresaId => Guid.Empty;
    }
}
