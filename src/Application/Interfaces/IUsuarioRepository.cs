using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato de persistência para o agregado Usuario.
/// </summary>
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Usuario?> ObterPorEmailAsync(Guid empresaId, string email, CancellationToken ct = default);
    Task<Usuario?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken ct = default);
    void Atualizar(Usuario usuario);
}
