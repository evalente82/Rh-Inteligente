using Application.Interfaces;
using Infrastructure.AI;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Ponto central de registro de todos os serviços da camada Infrastructure no DI do ASP.NET Core 8.
/// Chamado uma única vez em Program.cs via builder.Services.AddInfrastructure(builder.Configuration).
/// </summary>
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ─── Tenant Provider (multi-tenant via rota HTTP) ─────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, HttpContextTenantProvider>();

        // ─── DbContext ────────────────────────────────────────────────────────
        // A connection string é lida de "ConnectionStrings:Postgres" no appsettings.json
        services.AddDbContext<RhInteligenteDbContext>((serviceProvider, options) =>
        {
            var connectionString = configuration.GetConnectionString("Postgres")
                ?? throw new InvalidOperationException(
                    "Connection string 'Postgres' não encontrada no appsettings.json.");

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(RhInteligenteDbContext).Assembly.FullName);
                // Resiliência: retry automático em falhas transitórias do Postgres
                npgsql.EnableRetryOnFailure(maxRetryCount: 3);
            });
        });

        // Factory para o DataSeeder (usa SystemTenantProvider — sem filtro de tenant)
        // Registrada como Singleton usando as mesmas options do DbContext principal
        services.AddSingleton<IDbContextFactory<RhInteligenteDbContext>>(sp =>
        {
            var opts = sp.GetRequiredService<DbContextOptions<RhInteligenteDbContext>>();
            return new SeedDbContextFactory(opts);
        });

        services.AddTransient<DataSeeder>();

        // ─── Repositórios ─────────────────────────────────────────────────────
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IRegistroPontoRepository, RegistroPontoRepository>();
        services.AddScoped<IAdmissaoRepository, AdmissaoRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAlertaAnomaliaQueryRepository, AlertaAnomaliaQueryRepository>();
        services.AddScoped<IFechamentoFolhaRepository, FechamentoFolhaRepository>();
        services.AddScoped<IContrachequeRepository, ContrachequeRepository>();

        // ─── Unit of Work ─────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        // ─── Auth (M4) ────────────────────────────────────────────────────────
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<ISenhaHasher, BcryptSenhaHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // ─── Storage / Background (stubs para Dev — trocar em Prod) ──────────
        services.AddScoped<IArmazenamentoArquivoService, LocalArmazenamentoArquivoService>();
        services.AddScoped<IAnalisadorBackgroundService, NoOpAnalisadorBackgroundService>();

        // ─── IA / RAG (Módulo 3) ──────────────────────────────────────────────
        services.Configure<GeminiOptions>(
            configuration.GetSection(GeminiOptions.SecaoConfig));
        services.Configure<QdrantOptions>(
            configuration.GetSection(QdrantOptions.SecaoConfig));

        services.AddScoped<IAuditorIaService, GeminiAuditorIaService>();
        services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
        services.AddScoped<IVectorRepository, QdrantVectorRepository>();
        services.AddScoped<ICctPdfParser, CctPdfParserService>();

        return services;
    }
}
