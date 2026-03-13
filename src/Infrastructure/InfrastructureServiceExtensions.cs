using Application.Interfaces;
using Infrastructure.AI;
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

        // ─── Repositórios ─────────────────────────────────────────────────────
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IRegistroPontoRepository, RegistroPontoRepository>();
        services.AddScoped<IAdmissaoRepository, AdmissaoRepository>();

        // ─── Unit of Work ─────────────────────────────────────────────────────
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

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
