using API.Middleware;
using Application.UseCases;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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
