using API.Middleware;
using Application.UseCases;
using DotNetEnv;
using Infrastructure;

// ─── Carrega o .env da raiz do repositório ────────────────────────────────────
// O arquivo .env está dois níveis acima de src/API (raiz do repositório).
// Em produção o arquivo pode não existir (variáveis injetadas pelo host/CI-CD).
var raizRepo = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
var envFile = Path.Combine(raizRepo, ".env");
if (File.Exists(envFile))
    Env.Load(envFile);

var builder = WebApplication.CreateBuilder(args);

// ─── Injeta as variáveis do .env no IConfiguration por ambiente ───────────────
// Se ASPNETCORE_ENVIRONMENT=Development → usa prefixo DEV_
// Se ASPNETCORE_ENVIRONMENT=Production  → usa prefixo PROD_
var prefix = builder.Environment.IsProduction() ? "PROD_" : "DEV_";

// Connection String → sobrescreve appsettings
var pgHost   = Environment.GetEnvironmentVariable($"{prefix}POSTGRES_HOST");
var pgPort   = Environment.GetEnvironmentVariable($"{prefix}POSTGRES_PORT");
var pgDb     = Environment.GetEnvironmentVariable($"{prefix}POSTGRES_DATABASE");
var pgUser   = Environment.GetEnvironmentVariable($"{prefix}POSTGRES_USERNAME");
var pgPass   = Environment.GetEnvironmentVariable($"{prefix}POSTGRES_PASSWORD");

if (!string.IsNullOrWhiteSpace(pgHost))
{
    var connStr = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
    builder.Configuration["ConnectionStrings:Postgres"] = connStr;
}

// Gemini
var geminiKey   = Environment.GetEnvironmentVariable($"{prefix}GEMINI_API_KEY");
var geminiModel = Environment.GetEnvironmentVariable($"{prefix}GEMINI_MODEL");
if (!string.IsNullOrWhiteSpace(geminiKey))
    builder.Configuration["Gemini:ApiKey"] = geminiKey;
if (!string.IsNullOrWhiteSpace(geminiModel))
    builder.Configuration["Gemini:ModeloChat"] = geminiModel;

// Qdrant
var qdrantHost   = Environment.GetEnvironmentVariable($"{prefix}QDRANT_HOST");
var qdrantPort   = Environment.GetEnvironmentVariable($"{prefix}QDRANT_PORT");
var qdrantPrefix = Environment.GetEnvironmentVariable($"{prefix}QDRANT_PREFIX");
if (!string.IsNullOrWhiteSpace(qdrantHost))
    builder.Configuration["Qdrant:Host"] = qdrantHost;
if (!string.IsNullOrWhiteSpace(qdrantPort))
    builder.Configuration["Qdrant:Porta"] = qdrantPort;
if (!string.IsNullOrWhiteSpace(qdrantPrefix))
    builder.Configuration["Qdrant:AmbientePrefix"] = qdrantPrefix;

// ─── Serviços ────────────────────────────────────────────────────────────────

builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Vcorp Folha IA — API",
        Version = "v1",
        Description = "API de Auditoria Inteligente de Ponto e Folha (Módulo 1)"
    });
});

// Tratamento global de exceções nativo do ASP.NET Core 8
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Use Cases (Application Layer) ───────────────────────────────────────────
builder.Services.AddScoped<UploadResumoFolhaUseCase>();
builder.Services.AddScoped<AnalisarRegistrosComIaUseCase>();
builder.Services.AddScoped<CadastrarFuncionarioUseCase>();
builder.Services.AddScoped<AdmitirFuncionarioUseCase>();
builder.Services.AddScoped<ListarFuncionariosUseCase>();
builder.Services.AddScoped<DemitirFuncionarioUseCase>();
builder.Services.AddScoped<ProcessarCctUseCase>();
builder.Services.AddScoped<GerarHoleriteNarrativoUseCase>();

// ─── Infrastructure (EF Core + Repositórios + UnitOfWork) ────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ─── Pipeline ────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Vcorp Folha IA v1"));
}

// Middleware de exceções deve ser o primeiro da pipeline
app.UseExceptionHandler();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

// Necessário para os testes de integração acessarem o Program via WebApplicationFactory
public partial class Program { }
