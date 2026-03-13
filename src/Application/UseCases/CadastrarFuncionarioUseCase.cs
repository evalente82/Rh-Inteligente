using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.ValueObjects;

namespace Application.UseCases;

/// <summary>
/// Cadastra um novo funcionário no sistema (sem admissão formal ainda).
/// A admissão formal (com cargo, salário, regime) é feita pelo <see cref="AdmitirFuncionarioUseCase"/>.
/// </summary>
public sealed class CadastrarFuncionarioUseCase
{
    private readonly IFuncionarioRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CadastrarFuncionarioUseCase(
        IFuncionarioRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FuncionarioOutputDTO> ExecutarAsync(
        CadastrarFuncionarioInputDTO input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Valida CPF antes de qualquer persistência
        var cpf = new Cpf(input.Cpf);

        // Garante matrícula única por empresa
        var existente = await _repository.ObterPorMatriculaAsync(
            input.Matricula, input.EmpresaId, cancellationToken);

        if (existente is not null)
            throw new InvalidOperationException(
                $"Já existe um funcionário com a matrícula '{input.Matricula}' nesta empresa.");

        var turno = new TurnoTrabalho(
            TimeOnly.Parse(input.HoraEntrada),
            TimeOnly.Parse(input.HoraSaida),
            TimeSpan.FromMinutes(input.IntervaloAlmocoMinutos));

        var funcionario = Funcionario.Criar(
            input.EmpresaId,
            input.Nome,
            cpf,
            input.Matricula,
            input.DataAdmissao,
            turno);

        await _repository.AdicionarAsync(funcionario, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapearParaDTO(funcionario);
    }

    internal static FuncionarioOutputDTO MapearParaDTO(Funcionario f)
    {
        var admissao = f.AdmissaoAtiva;
        return new FuncionarioOutputDTO(
            Id: f.Id,
            EmpresaId: f.EmpresaId,
            Nome: f.Nome,
            Cpf: f.Cpf.NumeroFormatado,
            Matricula: f.Matricula,
            Ativo: f.Ativo,
            DataAdmissao: f.DataAdmissao,
            DataDemissao: f.DataDemissao,
            HoraEntrada: f.TurnoContratual.HoraEntrada.ToString("HH:mm"),
            HoraSaida: f.TurnoContratual.HoraSaida.ToString("HH:mm"),
            IntervaloAlmocoMinutos: (int)f.TurnoContratual.DuracaoIntervalo.TotalMinutes,
            Cargo: admissao?.Cargo,
            SalarioBase: admissao?.SalarioBase,
            Regime: admissao?.Regime);
    }
}
