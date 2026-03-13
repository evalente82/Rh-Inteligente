using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Realiza o desligamento formal de um funcionário:
/// demite o vínculo CLT/PJ na entidade Funcionario e encerra a Admissao ativa.
/// </summary>
public sealed class DemitirFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IAdmissaoRepository _admissaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DemitirFuncionarioUseCase(
        IFuncionarioRepository funcionarioRepository,
        IAdmissaoRepository admissaoRepository,
        IUnitOfWork unitOfWork)
    {
        _funcionarioRepository = funcionarioRepository;
        _admissaoRepository = admissaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecutarAsync(
        Guid empresaId,
        Guid funcionarioId,
        DateTime dataDemissao,
        CancellationToken cancellationToken = default)
    {
        if (empresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(empresaId));

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(
            funcionarioId, empresaId, cancellationToken)
            ?? throw new KeyNotFoundException($"Funcionário '{funcionarioId}' não encontrado.");

        if (!funcionario.Ativo)
            throw new InvalidOperationException("O funcionário já está demitido.");

        // Encerra a admissão ativa no domínio da Admissao
        var admissaoAtiva = await _admissaoRepository.ObterAdmissaoAtivaAsync(
            funcionarioId, empresaId, cancellationToken);

        if (admissaoAtiva is not null)
        {
            admissaoAtiva.Demitir(DateOnly.FromDateTime(dataDemissao));
            _admissaoRepository.Atualizar(admissaoAtiva);
        }

        // Registra a demissão na entidade principal
        funcionario.Demitir(dataDemissao);
        _funcionarioRepository.Atualizar(funcionario);

        await _unitOfWork.CommitAsync(cancellationToken);
    }
}
