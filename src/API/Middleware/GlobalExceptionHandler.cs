using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

/// <summary>
/// Tratamento global de exceções seguindo RFC 7807 (ProblemDetails).
/// Registrado via builder.Services.AddExceptionHandler&lt;GlobalExceptionHandler&gt;()
/// nativo do ASP.NET Core 8 — sem middlewares customizados legados.
/// </summary>
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, titulo) = exception switch
        {
            ArgumentException or ArgumentNullException
                => (StatusCodes.Status400BadRequest, "Requisição inválida"),

            InvalidOperationException
                => (StatusCodes.Status422UnprocessableEntity, "Operação não permitida"),

            KeyNotFoundException
                => (StatusCodes.Status404NotFound, "Recurso não encontrado"),

            UnauthorizedAccessException
                => (StatusCodes.Status403Forbidden, "Acesso negado"),

            _
                => (StatusCodes.Status500InternalServerError, "Erro interno no servidor")
        };

        _logger.LogError(
            exception,
            "Exceção não tratada: {ExceptionType} — {Message}",
            exception.GetType().Name,
            exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = titulo,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
