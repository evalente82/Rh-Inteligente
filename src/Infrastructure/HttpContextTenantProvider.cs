using Microsoft.AspNetCore.Http;

namespace Infrastructure;

/// <summary>
/// Implementação do ITenantProvider que resolve o EmpresaId a partir do
/// parâmetro de rota {empresaId} da requisição HTTP atual.
/// Padrão: a rota deve conter o segmento {empresaId:guid} (ex: /api/folhaponto/{empresaId}/...).
/// </summary>
internal sealed class HttpContextTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid EmpresaId
    {
        get
        {
            var routeValue = _httpContextAccessor.HttpContext?
                .Request.RouteValues["empresaId"]?.ToString();

            return Guid.TryParse(routeValue, out var empresaId)
                ? empresaId
                : Guid.Empty;
        }
    }
}
