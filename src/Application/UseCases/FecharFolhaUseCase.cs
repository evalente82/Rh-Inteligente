using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Services;

namespace Application.UseCases;

/// <summary>
/// Consolida o período de ponto, calcula horas extras e descontos,
/// invoca a IA para gerar o relatório narrativo e fecha a folha.
/// Retorna rapidamente com status 202 — o processamento real ocorre no background.
/// </summary>
public sealed class FecharFolhaUseCase
{
    private readonly IFechamentoFolhaRepository _fechamentoRepo;
    private readonly IFuncionarioRepository _funcionarioRepo;
    private readonly IRegistroPontoRepository _registroRepo;
    private readonly IAlertaAnomaliaQueryRepository _alertaRepo;
    private readonly CalculoHoraExtraService _calculoService;
    private readonly IUnitOfWork _uow;

    public FecharFolhaUseCase(
        IFechamentoFolhaRepository fechamentoRepo,
        IFuncionarioRepository funcionarioRepo,
        IRegistroPontoRepository registroRepo,
        IAlertaAnomaliaQueryRepository alertaRepo,
        CalculoHoraExtraService calculoService,
        IUnitOfWork uow)
    {
        _fechamentoRepo = fechamentoRepo;
        _funcionarioRepo = funcionarioRepo;
        _registroRepo = registroRepo;
        _alertaRepo = alertaRepo;
        _calculoService = calculoService;
        _uow = uow;
    }

    public async Task<FechamentoFolhaOutputDTO> ExecutarAsync(
        FecharFolhaInputDTO input,
        CancellationToken ct = default)
    {
        if (input.EmpresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(input));

        if (input.PeriodoFim <= input.PeriodoInicio)
            throw new ArgumentException("PeriodoFim deve ser posterior ao PeriodoInicio.");

        // Impede duplicidade de fechamento para o mesmo período
        var existente = await _fechamentoRepo.ObterAbertoPorPeriodoAsync(
            input.EmpresaId, input.PeriodoInicio, input.PeriodoFim, ct);

        if (existente is not null)
            throw new InvalidOperationException(
                $"Já existe um fechamento para o período {input.PeriodoInicio:dd/MM/yyyy} – {input.PeriodoFim:dd/MM/yyyy}.");

        // Agrega alertas do período para calcular anomalias críticas
        var alertas = await _alertaRepo.ListarPorPeriodoAsync(
            input.EmpresaId, input.PeriodoInicio, input.PeriodoFim, ct);

        var totalCriticos = alertas.Count(a => a.Gravidade == 3 && !a.Resolvido);

        // Agrega horas extras de todos os funcionários ativos no período
        var funcionarios = await _funcionarioRepo.ListarPorEmpresaAsync(input.EmpresaId, ct);
        var ativos = funcionarios.Where(f => f.Ativo).ToList();

        decimal totalHorasExtras = 0m;
        decimal totalDescontos = 0m;

        foreach (var funcionario in ativos)
        {
            var registros = await _registroRepo.ListarPorPeriodoAsync(
                funcionario.Id, input.EmpresaId,
                input.PeriodoInicio, input.PeriodoFim, ct);

            var registrosList = registros.ToList();
            if (registrosList.Count == 0) continue;

            // Agrupa por dia e acumula — CalculoHoraExtraService opera diariamente
            var porDia = registrosList.GroupBy(r => r.DataHoraBatida.Date);
            foreach (var grupo in porDia)
            {
                var resultado = _calculoService.Calcular(funcionario, grupo);
                totalHorasExtras += (decimal)resultado.HorasExtras.TotalHours;

                // Desconto = horas abaixo da carga contratual (atraso/falta parcial)
                var deficit = resultado.CargaContratual - resultado.JornadaEfetiva;
                if (deficit > TimeSpan.Zero)
                    totalDescontos += (decimal)deficit.TotalHours;
            }
        }

        // Narrativa sintética — gerada via IA no GerarRelatorioFolhaUseCase (pós-aprovação)
        // Aqui usamos um placeholder estruturado que a IA expandirá no GET /relatorio
        var narrativaInicial =
            $"Folha do período {input.PeriodoInicio:dd/MM/yyyy} a {input.PeriodoFim:dd/MM/yyyy}. " +
            $"Funcionários processados: {ativos.Count}. " +
            $"Horas extras acumuladas: {totalHorasExtras:F2}h. " +
            $"Descontos: {totalDescontos:F2}h. " +
            $"Anomalias críticas pendentes: {totalCriticos}.";

        var fechamento = FechamentoFolha.Abrir(
            input.EmpresaId,
            input.PeriodoInicio,
            input.PeriodoFim);

        fechamento.Fechar(
            Math.Round(totalHorasExtras, 2),
            Math.Round(totalDescontos, 2),
            totalCriticos,
            narrativaInicial);

        await _fechamentoRepo.AdicionarAsync(fechamento, ct);
        await _uow.CommitAsync(ct);

        return ToOutputDTO(fechamento);
    }

    internal static FechamentoFolhaOutputDTO ToOutputDTO(FechamentoFolha f) =>
        new(f.Id, f.EmpresaId, f.PeriodoInicio, f.PeriodoFim,
            f.Status.ToString(), f.TotalHorasExtras, f.TotalDescontos,
            f.TotalAnomaliasCriticas, f.RelatorioNarrativo,
            f.CriadaEm, f.FechadaEm, f.AprovadaEm);
}
