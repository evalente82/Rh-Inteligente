using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 do repositório de <see cref="Usuario"/>.
/// O Global Query Filter por EmpresaId no DbContext garante o isolamento multi-tenant.
/// </summary>
internal sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly RhInteligenteDbContext _context;

    public UsuarioRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<Usuario?> ObterPorEmailAsync(Guid empresaId, string email, CancellationToken ct = default)
        => await _context.Usuarios
            .IgnoreQueryFilters() // Busca cross-tenant necessária para auth
            .FirstOrDefaultAsync(u => u.EmpresaId == empresaId && u.Email.Endereco == email.ToLowerInvariant(), ct);

    public async Task<Usuario?> ObterPorRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        => await _context.Usuarios
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken, ct);

    public async Task AdicionarAsync(Usuario usuario, CancellationToken ct = default)
        => await _context.Usuarios.AddAsync(usuario, ct);

    public void Atualizar(Usuario usuario)
        => _context.Usuarios.Update(usuario);
}
