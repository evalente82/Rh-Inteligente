using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para o agregado Empresa (tenant).
/// </summary>
public interface IEmpresaRepository
{
    Task<Empresa?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Empresa?> ObterPorCnpjAsync(string cnpj, CancellationToken ct = default);
    Task AdicionarAsync(Empresa empresa, CancellationToken ct = default);
}
