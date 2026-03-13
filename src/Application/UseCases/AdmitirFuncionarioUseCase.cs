using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.UseCases;

/// <summary>
/// Formaliza a admissão de um funcionário já cadastrado,
/// registrando cargo, salário, regime e endereço (eSocial).
/// </summary>
public sealed class AdmitirFuncionarioUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IAdmissaoRepository _admissaoRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AdmitirFuncionarioUseCase(
        IFuncionarioRepository funcionarioRepository,
        IAdmissaoRepository admissaoRepository,
        IUnitOfWork unitOfWork)
    {
        _funcionarioRepository = funcionarioRepository;
        _admissaoRepository = admissaoRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FuncionarioOutputDTO> ExecutarAsync(
        AdmitirFuncionarioInputDTO input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(
            input.FuncionarioId, input.EmpresaId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"Funcionário '{input.FuncionarioId}' não encontrado.");

        // Valida que não há admissão ativa
        var admissaoAtiva = await _admissaoRepository.ObterAdmissaoAtivaAsync(
            input.FuncionarioId, input.EmpresaId, cancellationToken);

        if (admissaoAtiva is not null)
            throw new InvalidOperationException(
                "Funcionário já possui uma admissão ativa. Demita o vínculo atual antes de criar um novo.");

        var endereco = new Endereco(
            input.Logradouro, input.NumeroEndereco, input.Bairro,
            input.Cidade, input.Uf, input.Cep, input.Complemento);

        var admissao = Admissao.Criar(
            input.EmpresaId,
            input.FuncionarioId,
            input.Cargo,
            input.SalarioBase,
            input.Regime,
            input.DataAdmissao,
            endereco);

        await _admissaoRepository.AdicionarAsync(admissao, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return CadastrarFuncionarioUseCase.MapearParaDTO(funcionario) with
        {
            Cargo = admissao.Cargo,
            SalarioBase = admissao.SalarioBase,
            Regime = admissao.Regime
        };
    }
}
