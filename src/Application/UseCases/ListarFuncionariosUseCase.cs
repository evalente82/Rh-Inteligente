using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Lista todos os funcionários ativos de uma empresa (tenant).
/// </summary>
public sealed class ListarFuncionariosUseCase
{
    private readonly IFuncionarioRepository _repository;

    public ListarFuncionariosUseCase(IFuncionarioRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<FuncionarioOutputDTO>> ExecutarAsync(
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        var funcionarios = await _repository.ListarPorEmpresaAsync(empresaId, cancellationToken);

        return funcionarios.Select(CadastrarFuncionarioUseCase.MapearParaDTO);
    }

    public async Task<FuncionarioOutputDTO> ExecutarPorIdAsync(
        Guid id,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id não pode ser vazio.", nameof(id));

        var funcionario = await _repository.ObterPorIdAsync(id, empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Funcionário '{id}' não encontrado.");

        return CadastrarFuncionarioUseCase.MapearParaDTO(funcionario);
    }
}
