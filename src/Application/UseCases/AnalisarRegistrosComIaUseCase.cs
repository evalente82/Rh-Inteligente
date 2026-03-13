using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases;

/// <summary>
/// Caso de Uso: orquestra a análise de IA sobre os registros de ponto de um
/// funcionário em um período. Executado EXCLUSIVAMENTE em background job
/// (nunca chamado diretamente por um Controller — Regra 3 do Roadmap).
///
/// Fluxo:
///   1. Valida os dados de entrada.
///   2. Busca o Funcionario (com TurnoContratual) no repositório.
///   3. Busca todos os RegistrosPonto do período.
///   4. Envia ao IAuditorIaService para análise e retorna os AlertaAnomalia.
///   5. Persiste os alertas e commita a transação via IUnitOfWork.
///   6. Retorna a lista de DTOs de alertas para o chamador (job/worker).
/// </summary>
public sealed class AnalisarRegistrosComIaUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IRegistroPontoRepository _registroRepository;
    private readonly IAuditorIaService _auditorIa;
    private readonly IUnitOfWork _unitOfWork;

    public AnalisarRegistrosComIaUseCase(
        IFuncionarioRepository funcionarioRepository,
        IRegistroPontoRepository registroRepository,
        IAuditorIaService auditorIa,
        IUnitOfWork unitOfWork)
    {
        _funcionarioRepository = funcionarioRepository;
        _registroRepository = registroRepository;
        _auditorIa = auditorIa;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Executa a análise completa e retorna os alertas gerados pela IA.
    /// </summary>
    public async Task<IEnumerable<AlertaAnomaliaOutputDTO>> ExecutarAsync(
        AnalisarRegistrosInputDTO input,
        CancellationToken cancellationToken = default)
    {
        ValidarInput(input);

        // Passo 1: carrega o funcionário (com turno contratual)
        var funcionario = await _funcionarioRepository.ObterPorIdAsync(
            input.FuncionarioId, input.EmpresaId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Funcionário '{input.FuncionarioId}' não encontrado para a empresa '{input.EmpresaId}'.");

        // Passo 2: carrega os registros de ponto do período
        var registros = await _registroRepository.ListarPorPeriodoAsync(
            input.FuncionarioId,
            input.EmpresaId,
            input.PeriodoInicio,
            input.PeriodoFim,
            cancellationToken);

        // Passo 3: delega a análise à IA
        var alertas = await _auditorIa.AnalisarAsync(funcionario, registros, cancellationToken);

        var listaAlertas = alertas.ToList();

        // Passo 4: commita (a persistência dos alertas é responsabilidade
        // da implementação concreta de IAuditorIaService + IUnitOfWork na Infrastructure)
        await _unitOfWork.CommitAsync(cancellationToken);

        // Passo 5: mapeia para DTO de saída (não expõe entidade de domínio)
        return listaAlertas.Select(MapearParaDTO);
    }

    // --- Mapeamento para DTO ---

    private static AlertaAnomaliaOutputDTO MapearParaDTO(AlertaAnomalia alerta) =>
        new(
            Id: alerta.Id,
            FuncionarioId: alerta.FuncionarioId,
            TipoAnomalia: alerta.TipoAnomalia,
            DataReferencia: alerta.DataReferencia,
            Descricao: alerta.Descricao,
            Gravidade: alerta.Gravidade,
            GeradoEm: alerta.GeradoEm,
            Resolvido: alerta.Resolvido,
            ResolvidoEm: alerta.ResolvidoEm
        );

    // --- Validações de invariantes de entrada ---

    private static void ValidarInput(AnalisarRegistrosInputDTO input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.EmpresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(input));

        if (input.FuncionarioId == Guid.Empty)
            throw new ArgumentException("FuncionarioId não pode ser vazio.", nameof(input));

        if (input.PeriodoFim < input.PeriodoInicio)
            throw new ArgumentException(
                "A data fim do período não pode ser anterior à data início.", nameof(input));
    }
}
