using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação EF Core 8 do repositório de <see cref="Funcionario"/>.
/// O Global Query Filter por EmpresaId no DbContext garante o isolamento multi-tenant
/// automaticamente — os métodos não precisam filtrar manualmente por EmpresaId.
/// O parâmetro empresaId nos métodos é mantido para conformidade com a interface
/// (útil em cenários sem o filter ativo, como testes de integração com IgnoreQueryFilters).
/// </summary>
internal sealed class FuncionarioRepository : IFuncionarioRepository
{
    private readonly RhInteligenteDbContext _context;

    public FuncionarioRepository(RhInteligenteDbContext context)
    {
        _context = context;
    }

    public async Task<Funcionario?> ObterPorIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Funcionarios
            .Include(f => f.RegistrosPonto)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Funcionario?> ObterPorMatriculaAsync(
        string matricula,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Funcionarios
            .FirstOrDefaultAsync(f => f.Matricula == matricula, cancellationToken);
    }

    public async Task<IEnumerable<Funcionario>> ListarPorEmpresaAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Funcionarios
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(
        Funcionario funcionario,
        CancellationToken cancellationToken = default)
    {
        await _context.Funcionarios.AddAsync(funcionario, cancellationToken);
    }

    public void Atualizar(Funcionario funcionario)
    {
        _context.Funcionarios.Update(funcionario);
    }
}
