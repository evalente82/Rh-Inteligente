using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Services;

namespace Application.UseCases;

/// <summary>
/// Gera os contracheques de todos os funcionários ativos de um fechamento de folha.
/// Para cada funcionário:
///   1. Obtém salário base + horas extras do FechamentoFolha
///   2. Calcula INSS, IRRF e FGTS via CalculoEncargosFolhaService
///   3. Persiste um Contracheque com todos os ItemContracheque
/// Idempotente: ignora funcionários que já possuem contracheque no fechamento.
/// </summary>
public sealed class GerarContrachequeUseCase
{
    private readonly IContrachequeRepository _contrachequeRepository;
    private readonly IFechamentoFolhaRepository _fechamentoRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IAdmissaoRepository _admissaoRepository;
    private readonly CalculoEncargosFolhaService _calculoEncargos;
    private readonly IUnitOfWork _uow;

    public GerarContrachequeUseCase(
        IContrachequeRepository contrachequeRepository,
        IFechamentoFolhaRepository fechamentoRepository,
        IFuncionarioRepository funcionarioRepository,
        IAdmissaoRepository admissaoRepository,
        CalculoEncargosFolhaService calculoEncargos,
        IUnitOfWork uow)
    {
        _contrachequeRepository = contrachequeRepository;
        _fechamentoRepository   = fechamentoRepository;
        _funcionarioRepository  = funcionarioRepository;
        _admissaoRepository     = admissaoRepository;
        _calculoEncargos        = calculoEncargos;
        _uow                    = uow;
    }

    /// <summary>
    /// Gera (ou complementa) os contracheques do fechamento.
    /// </summary>
    /// <returns>Lista dos contracheques gerados nesta execução (os já existentes são ignorados).</returns>
    public async Task<IReadOnlyCollection<ContrachequeOutputDTO>> ExecutarAsync(
        GerarContrachequeInputDTO input,
        CancellationToken ct = default)
    {
        if (input.EmpresaId == Guid.Empty)
            throw new ArgumentException("EmpresaId não pode ser vazio.", nameof(input));
        if (input.FechamentoFolhaId == Guid.Empty)
            throw new ArgumentException("FechamentoFolhaId não pode ser vazio.", nameof(input));

        // Valida existência do fechamento
        var fechamento = await _fechamentoRepository.ObterPorIdAsync(input.FechamentoFolhaId, ct)
            ?? throw new KeyNotFoundException(
                $"FechamentoFolha '{input.FechamentoFolhaId}' não encontrado.");

        // Competência no formato "MM/YYYY" derivada do período de início
        var competencia = $"{fechamento.PeriodoInicio.Month:D2}/{fechamento.PeriodoInicio.Year}";

        var funcionarios = (await _funcionarioRepository.ListarPorEmpresaAsync(input.EmpresaId, ct))
            .Where(f => f.Ativo)
            .ToList();

        var gerados = new List<ContrachequeOutputDTO>();

        foreach (var funcionario in funcionarios)
        {
            // Idempotência: se já existe contracheque para este funcionário+fechamento, pula
            var jaExiste = await _contrachequeRepository.ExistePorFuncionarioEFechamentoAsync(
                funcionario.Id, input.FechamentoFolhaId, ct);

            if (jaExiste) continue;

            // Obtém admissão ativa para o salário base
            var admissao = await _admissaoRepository.ObterAdmissaoAtivaAsync(
                funcionario.Id, input.EmpresaId, ct);

            if (admissao is null) continue; // funcionário sem admissão formal — não gera contracheque

            // Salário bruto = salário base
            // Em versão futura: somar HE do FechamentoFolha por funcionário quando disponível
            var salarioBase = admissao.SalarioBase;

            var encargos = _calculoEncargos.Calcular(salarioBase);

            var contracheque = Contracheque.Criar(
                input.EmpresaId,
                input.FechamentoFolhaId,
                funcionario.Id,
                competencia);

            // ─── Proventos ────────────────────────────────────────────────────
            contracheque.AdicionarItem(TipoRubrica.SalarioBase, "Salário Base", salarioBase);

            // ─── Descontos legais ─────────────────────────────────────────────
            if (encargos.InssDevido > 0)
            {
                var aliquotaInss = ObterAliquotaInssDescricao(salarioBase);
                contracheque.AdicionarItem(TipoRubrica.DescontoInss, $"INSS ({aliquotaInss})", encargos.InssDevido);
            }

            if (encargos.IrrfDevido > 0)
                contracheque.AdicionarItem(TipoRubrica.DescontoIrrf, "IRRF", encargos.IrrfDevido);

            // ─── Desconto por atraso do FechamentoFolha ───────────────────────
            if (fechamento.TotalDescontos > 0)
                contracheque.AdicionarItem(TipoRubrica.DescontoAtraso, "Desconto por Atraso/Ausência",
                    fechamento.TotalDescontos / Math.Max(funcionarios.Count, 1));

            // ─── FGTS informativo (encargo patronal) ──────────────────────────
            contracheque.AdicionarItem(TipoRubrica.FgtsInformativo, "FGTS (8% — encargo patronal)", encargos.FgtsPatronal);

            await _contrachequeRepository.AdicionarAsync(contracheque, ct);
            gerados.Add(ToOutputDTO(contracheque));
        }

        if (gerados.Count > 0)
            await _uow.CommitAsync(ct);

        return gerados.AsReadOnly();
    }

    internal static ContrachequeOutputDTO ToOutputDTO(Contracheque c) =>
        new(
            c.Id,
            c.EmpresaId,
            c.FechamentoFolhaId,
            c.FuncionarioId,
            c.Competencia,
            c.SalarioBruto,
            c.TotalDescontos,
            c.SalarioLiquido,
            c.FgtsPatronal,
            c.GeradoEm,
            c.Itens.Select(i => new ItemContrachequeOutputDTO(
                i.Tipo.ToString(),
                i.Descricao,
                i.Valor,
                i.EhDesconto)).ToList().AsReadOnly());

    // Descrição textual da alíquota INSS para exibição no holerite
    private static string ObterAliquotaInssDescricao(decimal salarioBruto) =>
        salarioBruto switch
        {
            <= 1_412.00m  => "7,5%",
            <= 2_666.68m  => "9%",
            <= 4_000.03m  => "12%",
            <= 7_786.02m  => "14%",
            _             => "Teto"
        };
}
