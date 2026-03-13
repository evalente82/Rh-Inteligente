using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Gera um holerite narrativo para um funcionário em um período determinado.
/// Orquestra: busca de registros → auditoria IA → formatação do texto narrativo.
/// Chamado de forma síncrona (é leve — apenas leitura + 1 chamada ao Gemini).
/// </summary>
public sealed class GerarHoleriteNarrativoUseCase
{
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IRegistroPontoRepository _registroPontoRepository;
    private readonly IAuditorIaService _auditorIaService;

    public GerarHoleriteNarrativoUseCase(
        IFuncionarioRepository funcionarioRepository,
        IRegistroPontoRepository registroPontoRepository,
        IAuditorIaService auditorIaService)
    {
        _funcionarioRepository = funcionarioRepository;
        _registroPontoRepository = registroPontoRepository;
        _auditorIaService = auditorIaService;
    }

    public async Task<HoleriteNarrativoOutputDTO> ExecutarAsync(
        Guid empresaId,
        Guid funcionarioId,
        DateOnly dataInicio,
        DateOnly dataFim,
        CancellationToken ct = default)
    {
        if (dataFim < dataInicio)
            throw new ArgumentException("A data fim deve ser posterior à data início.");

        var funcionario = await _funcionarioRepository.ObterPorIdAsync(
            funcionarioId, empresaId, ct)
            ?? throw new KeyNotFoundException($"Funcionário '{funcionarioId}' não encontrado.");

        var registros = await _registroPontoRepository.ListarPorPeriodoAsync(
            funcionarioId, empresaId, dataInicio, dataFim, ct);

        var alertas = (await _auditorIaService.AnalisarAsync(funcionario, registros, ct)).ToList();

        // Conta alertas de hora extra como proxy para o total de HE no período
        var totalHorasExtras = alertas
            .Count(a => a.TipoAnomalia == Domain.Enums.TipoAnomalia.HoraExtraInesperada) * 0.5m;

        var periodo = $"{dataInicio:dd/MM/yyyy} a {dataFim:dd/MM/yyyy}";

        // O texto narrativo é gerado dentro do GeminiAuditorIaService e encapsulado no primeiro
        // alerta do tipo "Resumo". Para este MVP, construímos o texto a partir dos alertas.
        var narrativa = MontarNarrativa(funcionario.Nome, periodo, alertas.Count, totalHorasExtras);

        return new HoleriteNarrativoOutputDTO(
            FuncionarioId: funcionarioId,
            EmpresaId: empresaId,
            NomeFuncionario: funcionario.Nome,
            Periodo: periodo,
            TextoNarrativo: narrativa,
            TotalAnomalias: alertas.Count,
            TotalHorasExtras: totalHorasExtras,
            GeradoEm: DateTime.UtcNow);
    }

    private static string MontarNarrativa(
        string nome,
        string periodo,
        int totalAnomalias,
        decimal totalHorasExtras)
    {
        return totalAnomalias == 0
            ? $"O funcionário {nome} não apresentou anomalias no período {periodo}. " +
              "Todos os registros estão em conformidade com a convenção coletiva aplicável."
            : $"O funcionário {nome} apresentou {totalAnomalias} ocorrência(s) no período {periodo}, " +
              $"totalizando {totalHorasExtras:F1}h de horas extras identificadas. " +
              "Recomenda-se revisão dos alertas gerados pela auditoria de IA antes do fechamento da folha.";
    }
}
