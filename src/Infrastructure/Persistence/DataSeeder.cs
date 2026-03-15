using BCrypt.Net;
using Domain.Entities;
using Domain.Enums;
using Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

/// <summary>
/// Popula o banco com dados de demonstração para as duas empresas-tenant.
/// Executado na inicialização da API — idempotente (verifica existência antes de inserir).
///
/// ╔══════════════════════════════════════════════════════════════════════╗
/// ║  EMPRESA 1 — "Vcorp Tecnologia Ltda"                                ║
/// ║  EmpresaId : a1b2c3d4-0000-0000-0000-000000000001                  ║
/// ║  ┌─────────────────────────────────┬──────────────────────┐         ║
/// ║  │ Role        │ E-mail            │ Senha                │         ║
/// ║  ├─────────────────────────────────┼──────────────────────┤         ║
/// ║  │ Dono        │ dono@vcorp.com    │ Vcorp@2026!          │         ║
/// ║  │ Gestor      │ gestor@vcorp.com  │ Gestor@2026!         │         ║
/// ║  │ Colaborador │ joao@vcorp.com    │ Colab@2026!          │         ║
/// ║  └─────────────────────────────────┴──────────────────────┘         ║
/// ╠══════════════════════════════════════════════════════════════════════╣
/// ║  EMPRESA 2 — "Inova Serviços S/A"                                   ║
/// ║  EmpresaId : b2c3d4e5-0000-0000-0000-000000000002                  ║
/// ║  ┌─────────────────────────────────┬──────────────────────┐         ║
/// ║  │ Role        │ E-mail            │ Senha                │         ║
/// ║  ├─────────────────────────────────┼──────────────────────┤         ║
/// ║  │ Dono        │ dono@inova.com    │ Inova@2026!          │         ║
/// ║  │ Gestor      │ gestor@inova.com  │ Gestor@2026!         │         ║
/// ║  │ Colaborador │ maria@inova.com   │ Colab@2026!          │         ║
/// ║  └─────────────────────────────────┴──────────────────────┘         ║
/// ╚══════════════════════════════════════════════════════════════════════╝
/// </summary>
public sealed class DataSeeder
{
    // ─── IDs fixos (determinísticos) ──────────────────────────────────────────

    // Empresas
    public static readonly Guid EmpresaVcorpId = new("a1b2c3d4-0000-0000-0000-000000000001");
    public static readonly Guid EmpresaInovaId = new("b2c3d4e5-0000-0000-0000-000000000002");

    // Usuários — Vcorp
    private static readonly Guid UsuarioDonoVcorpId   = new("a1b2c3d4-0001-0000-0000-000000000001");
    private static readonly Guid UsuarioGestorVcorpId = new("a1b2c3d4-0001-0000-0000-000000000002");
    private static readonly Guid UsuarioColabVcorpId  = new("a1b2c3d4-0001-0000-0000-000000000003");

    // Usuários — Inova
    private static readonly Guid UsuarioDonoInovaId   = new("b2c3d4e5-0001-0000-0000-000000000001");
    private static readonly Guid UsuarioGestorInovaId = new("b2c3d4e5-0001-0000-0000-000000000002");
    private static readonly Guid UsuarioColabInovaId  = new("b2c3d4e5-0001-0000-0000-000000000003");

    private readonly IDbContextFactory<RhInteligenteDbContext> _factory;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(IDbContextFactory<RhInteligenteDbContext> factory, ILogger<DataSeeder> logger)
    {
        _factory = factory;
        _logger  = logger;
    }

    /// <summary>
    /// Ponto de entrada principal. Verifica se os dados já existem antes de inserir.
    /// Cria um DbContext próprio com SystemTenantProvider (Guid.Empty) para ignorar
    /// os Global Query Filters de tenant durante o seed.
    /// </summary>
    public async Task SeedAsync(CancellationToken ct = default)
    {
        await using var db = await _factory.CreateDbContextAsync(ct);

        // Aplica migrations pendentes automaticamente
        await db.Database.MigrateAsync(ct);

        // Idempotência: se já existe qualquer empresa, o seed foi executado
        var jaExiste = await db.Empresas.AnyAsync(ct);
        if (jaExiste)
        {
            _logger.LogInformation("[DataSeeder] Dados de seed já existem. Nenhuma ação necessária.");
            return;
        }

        _logger.LogInformation("[DataSeeder] Iniciando seed de demonstração...");

        await SeedEmpresaVcorpAsync(db, ct);
        await SeedEmpresaInovaAsync(db, ct);

        await db.SaveChangesAsync(ct);
        _logger.LogInformation("[DataSeeder] Seed concluído com sucesso.");
    }

    // =========================================================================
    // EMPRESA 1 — Vcorp Tecnologia Ltda (5 funcionários + cenários completos)
    // =========================================================================
    private async Task SeedEmpresaVcorpAsync(RhInteligenteDbContext db, CancellationToken ct)
    {
        // ─── Empresa ─────────────────────────────────────────────────────────
        var empresa = CriarEmpresaComId(
            EmpresaVcorpId,
            "Vcorp Tecnologia Ltda",
            "11222333000181",
            "contato@vcorp.com");
        await db.Empresas.AddAsync(empresa, ct);

        // ─── Usuários ─────────────────────────────────────────────────────────
        await db.Usuarios.AddRangeAsync(new[]
        {
            CriarUsuario(UsuarioDonoVcorpId,   EmpresaVcorpId, "dono@vcorp.com",   "Vcorp@2026!",  "Carlos Dono Silva",    Role.Dono),
            CriarUsuario(UsuarioGestorVcorpId, EmpresaVcorpId, "gestor@vcorp.com", "Gestor@2026!", "Ana Gestora Lima",     Role.Gestor),
            CriarUsuario(UsuarioColabVcorpId,  EmpresaVcorpId, "joao@vcorp.com",   "Colab@2026!",  "João Colaborador Melo",Role.Colaborador),
        }, ct);

        // ─── Endereço padrão ─────────────────────────────────────────────────
        var endSP = new Endereco("Av. Paulista", "1000", "Bela Vista", "São Paulo", "SP", "01310100");

        // ─── Turno padrão 8h–17h ─────────────────────────────────────────────
        var turno8h17h = new TurnoTrabalho(
            new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));

        // Turno diferente para dev senior
        var turno9h18h = new TurnoTrabalho(
            new TimeOnly(9, 0), new TimeOnly(18, 0), TimeSpan.FromHours(1));

        // ─── Funcionários (5 para Vcorp) ─────────────────────────────────────
        var f1 = CriarFuncionario(EmpresaVcorpId, "João Colaborador Melo",   "529.982.247-25", "VCP001", turno8h17h);
        var f2 = CriarFuncionario(EmpresaVcorpId, "Fernanda Rocha Santos",   "321.660.370-80", "VCP002", turno8h17h);
        var f3 = CriarFuncionario(EmpresaVcorpId, "Rafael Teixeira Costa",   "153.715.720-00", "VCP003", turno9h18h);
        var f4 = CriarFuncionario(EmpresaVcorpId, "Priscila Andrade Nunes",  "274.537.950-08", "VCP004", turno8h17h);
        var f5 = CriarFuncionario(EmpresaVcorpId, "Marcus Vinicius Pereira", "056.244.760-00", "VCP005", turno9h18h);

        await db.Funcionarios.AddRangeAsync(new[] { f1, f2, f3, f4, f5 }, ct);

        // ─── Admissões ───────────────────────────────────────────────────────
        var admissoes = new[]
        {
            CriarAdmissao(EmpresaVcorpId, f1.Id, "Analista de Sistemas",      3_200.00m, RegimeContratacao.Clt,       new DateOnly(2024, 3, 1), endSP),
            CriarAdmissao(EmpresaVcorpId, f2.Id, "Designer UX/UI",            2_900.00m, RegimeContratacao.Clt,       new DateOnly(2024, 6, 1), endSP),
            CriarAdmissao(EmpresaVcorpId, f3.Id, "Desenvolvedor Senior",      7_500.00m, RegimeContratacao.Clt,       new DateOnly(2023, 1,15), endSP),
            CriarAdmissao(EmpresaVcorpId, f4.Id, "Analista de RH",            2_500.00m, RegimeContratacao.Clt,       new DateOnly(2024,10, 1), endSP),
            CriarAdmissao(EmpresaVcorpId, f5.Id, "Consultor de Produto",      5_000.00m, RegimeContratacao.Pj,        new DateOnly(2025, 2, 1), endSP),
        };
        await db.Admissoes.AddRangeAsync(admissoes, ct);

        // ─── Fechamento de Folha — Fevereiro/2026 ────────────────────────────
        var fechamentoFev = CriarFechamento(EmpresaVcorpId,
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28),
            totalHE: 12.5m, totalDesc: 0.8m, totalCrit: 2);

        // ─── Fechamento de Folha — Janeiro/2026 ──────────────────────────────
        var fechamentoJan = CriarFechamento(EmpresaVcorpId,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31),
            totalHE: 8.0m, totalDesc: 0.0m, totalCrit: 1);

        await db.FechamentosFolha.AddRangeAsync(new[] { fechamentoFev, fechamentoJan }, ct);

        // ─── Contracheques Fevereiro/2026 ────────────────────────────────────
        foreach (var (adm, func) in admissoes.Zip(new[] { f1, f2, f3, f4, f5 }))
        {
            await db.Contracheques.AddAsync(
                GerarContracheque(EmpresaVcorpId, fechamentoFev.Id, func.Id, adm.SalarioBase, "02/2026"), ct);
        }

        // ─── Contracheques Janeiro/2026 ──────────────────────────────────────
        foreach (var (adm, func) in admissoes.Zip(new[] { f1, f2, f3, f4, f5 }))
        {
            await db.Contracheques.AddAsync(
                GerarContracheque(EmpresaVcorpId, fechamentoJan.Id, func.Id, adm.SalarioBase, "01/2026"), ct);
        }

        // ─── Alertas de Anomalia ─────────────────────────────────────────────
        await db.AlertasAnomalia.AddRangeAsync(new[]
        {
            CriarAlerta(EmpresaVcorpId, f1.Id, TipoAnomalia.HoraExtraInesperada,    "HE de 2h registrada sem autorização em 15/02/2026.", 2),
            CriarAlerta(EmpresaVcorpId, f2.Id, TipoAnomalia.IntervaloInsuficiente,  "Intervalo de apenas 28 minutos em 10/02/2026.",       2),
            CriarAlerta(EmpresaVcorpId, f3.Id, TipoAnomalia.HoraExtraInesperada,    "HE acima do limite CCT em 20/02/2026.",               3),
            CriarAlerta(EmpresaVcorpId, f4.Id, TipoAnomalia.FaltaDeRegistro,        "Falta em 07/02/2026 sem atestado.",                   2),
            CriarAlerta(EmpresaVcorpId, f5.Id, TipoAnomalia.BatidaForaDeSequencia,  "Entrada registrada 45 min antes do turno.",           1),
        }, ct);
    }

    // =========================================================================
    // EMPRESA 2 — Inova Serviços S/A (5 funcionários + cenários variados)
    // =========================================================================
    private async Task SeedEmpresaInovaAsync(RhInteligenteDbContext db, CancellationToken ct)
    {
        var empresa = CriarEmpresaComId(
            EmpresaInovaId,
            "Inova Serviços S/A",
            "44555666000195",
            "contato@inova.com");
        await db.Empresas.AddAsync(empresa, ct);

        // ─── Usuários ─────────────────────────────────────────────────────────
        await db.Usuarios.AddRangeAsync(new[]
        {
            CriarUsuario(UsuarioDonoInovaId,   EmpresaInovaId, "dono@inova.com",   "Inova@2026!",  "Ricardo Dono Alves",    Role.Dono),
            CriarUsuario(UsuarioGestorInovaId, EmpresaInovaId, "gestor@inova.com", "Gestor@2026!", "Beatriz Gestora Cunha", Role.Gestor),
            CriarUsuario(UsuarioColabInovaId,  EmpresaInovaId, "maria@inova.com",  "Colab@2026!",  "Maria Colaboradora Luz",Role.Colaborador),
        }, ct);

        var endRJ = new Endereco("Rua das Flores", "250", "Centro", "Rio de Janeiro", "RJ", "20040020");

        var turno8h17h = new TurnoTrabalho(
            new TimeOnly(8, 0), new TimeOnly(17, 0), TimeSpan.FromHours(1));
        var turnoNoturno = new TurnoTrabalho(
            new TimeOnly(14, 0), new TimeOnly(22, 0), TimeSpan.FromMinutes(30));

        // ─── Funcionários (5 para Inova) ─────────────────────────────────────
        var g1 = CriarFuncionario(EmpresaInovaId, "Maria Colaboradora Luz",   "263.946.010-60", "INV001", turno8h17h);
        var g2 = CriarFuncionario(EmpresaInovaId, "Pedro Henrique Barros",    "820.471.740-39", "INV002", turno8h17h);
        var g3 = CriarFuncionario(EmpresaInovaId, "Larissa Motta Carvalho",   "434.828.570-04", "INV003", turnoNoturno);
        var g4 = CriarFuncionario(EmpresaInovaId, "Rodrigo Farias Duarte",    "113.827.130-90", "INV004", turno8h17h);
        var g5 = CriarFuncionario(EmpresaInovaId, "Camila Nascimento Torres", "753.220.530-42", "INV005", turnoNoturno);

        await db.Funcionarios.AddRangeAsync(new[] { g1, g2, g3, g4, g5 }, ct);

        var admissoes = new[]
        {
            CriarAdmissao(EmpresaInovaId, g1.Id, "Assistente Administrativo", 2_200.00m, RegimeContratacao.Clt,    new DateOnly(2024, 5, 1),  endRJ),
            CriarAdmissao(EmpresaInovaId, g2.Id, "Coordenador de Logística",  4_500.00m, RegimeContratacao.Clt,    new DateOnly(2023, 8, 15), endRJ),
            CriarAdmissao(EmpresaInovaId, g3.Id, "Atendente de Suporte",      1_800.00m, RegimeContratacao.Clt,    new DateOnly(2025, 1, 1),  endRJ),
            CriarAdmissao(EmpresaInovaId, g4.Id, "Gerente Financeiro",        8_500.00m, RegimeContratacao.Clt,    new DateOnly(2022, 3, 1),  endRJ),
            CriarAdmissao(EmpresaInovaId, g5.Id, "Analista de Dados",         3_800.00m, RegimeContratacao.Estagio,new DateOnly(2025, 6, 1),  endRJ),
        };
        await db.Admissoes.AddRangeAsync(admissoes, ct);

        var fechamentoFev = CriarFechamento(EmpresaInovaId,
            new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 28),
            totalHE: 5.0m, totalDesc: 2.5m, totalCrit: 3);

        await db.FechamentosFolha.AddAsync(fechamentoFev, ct);

        foreach (var (adm, func) in admissoes.Zip(new[] { g1, g2, g3, g4, g5 }))
        {
            await db.Contracheques.AddAsync(
                GerarContracheque(EmpresaInovaId, fechamentoFev.Id, func.Id, adm.SalarioBase, "02/2026"), ct);
        }

        await db.AlertasAnomalia.AddRangeAsync(new[]
        {
            CriarAlerta(EmpresaInovaId, g1.Id, TipoAnomalia.FaltaDeRegistro,         "Falta em 03/02/2026 sem registro.",       2),
            CriarAlerta(EmpresaInovaId, g2.Id, TipoAnomalia.HoraExtraInesperada,     "2h extra sem compensação na semana.",     3),
            CriarAlerta(EmpresaInovaId, g3.Id, TipoAnomalia.BatidaForaDeSequencia,   "Saída não registrada em 18/02/2026.",     2),
            CriarAlerta(EmpresaInovaId, g4.Id, TipoAnomalia.IntervaloInsuficiente,   "Intervalo inferior a 1h em dia normal.", 1),
            CriarAlerta(EmpresaInovaId, g5.Id, TipoAnomalia.JornadaExcedida,         "Jornada de 10h registrada em 14/02.",    3),
        }, ct);
    }

    // =========================================================================
    // Helpers privados
    // =========================================================================

    private static Empresa CriarEmpresaComId(Guid id, string nome, string cnpj, string email)
    {
        // Reflexão mínima via construtor privado para forçar o ID determinístico
        var empresa = (Empresa)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(Empresa));

        SetPrivateProp(empresa, "Id", id);
        SetPrivateProp(empresa, "NomeFantasia", nome);
        SetPrivateProp(empresa, "Cnpj", cnpj);
        SetPrivateProp(empresa, "EmailContato", email);
        SetPrivateProp(empresa, "CriadaEm", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        SetPrivateProp(empresa, "Ativa", true);

        return empresa;
    }

    private static Usuario CriarUsuario(Guid id, Guid empresaId, string emailStr, string senha, string nome, Role role)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);
        var emailVo = Email.Criar(emailStr);

        var u = (Usuario)System.Runtime.CompilerServices.RuntimeHelpers
            .GetUninitializedObject(typeof(Usuario));

        SetPrivateProp(u, "Id", id);
        SetPrivateProp(u, "EmpresaId", empresaId);
        SetPrivateProp(u, "Email", emailVo);
        SetPrivateProp(u, "SenhaHash", hash);
        SetPrivateProp(u, "NomeCompleto", nome);
        SetPrivateProp(u, "Role", role);
        SetPrivateProp(u, "CriadoEm", new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        SetPrivateProp(u, "Ativo", true);

        return u;
    }

    private static Funcionario CriarFuncionario(
        Guid empresaId, string nome, string cpfStr, string matricula, TurnoTrabalho turno)
    {
        var cpf = new Cpf(cpfStr);
        return Funcionario.Criar(empresaId, nome, cpf, matricula,
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), turno);
    }

    private static Admissao CriarAdmissao(
        Guid empresaId, Guid funcionarioId, string cargo, decimal salario,
        RegimeContratacao regime, DateOnly dataAdmissao, Endereco endereco)
    {
        return Admissao.Criar(empresaId, funcionarioId, cargo, salario, regime, dataAdmissao, endereco);
    }

    private static FechamentoFolha CriarFechamento(
        Guid empresaId, DateOnly inicio, DateOnly fim,
        decimal totalHE, decimal totalDesc, int totalCrit)
    {
        var f = FechamentoFolha.Abrir(empresaId, inicio, fim);

        // Simula o fechamento com os dados de demonstração via reflexão
        SetPrivateProp(f, "TotalHorasExtras", totalHE);
        SetPrivateProp(f, "TotalDescontos", totalDesc);
        SetPrivateProp(f, "TotalAnomaliasCriticas", totalCrit);
        SetPrivateProp(f, "Status", StatusFolha.Fechada);
        SetPrivateProp(f, "FechadaEm", (DateTime?)new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
        SetPrivateProp(f, "RelatorioNarrativo",
            $"Período {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}: {totalHE}h extras registradas, " +
            $"{totalCrit} anomalias críticas identificadas. Folha aprovada para pagamento.");

        return f;
    }

    private static Contracheque GerarContracheque(
        Guid empresaId, Guid fechamentoId, Guid funcionarioId, decimal salarioBase, string competencia)
    {
        var c = Contracheque.Criar(empresaId, fechamentoId, funcionarioId, competencia);

        // INSS progressivo 2024 (simplificado para seed)
        var inss = CalcularInssSimples(salarioBase);
        var irrf = CalcularIrrfSimples(salarioBase - inss);
        var fgts = Math.Round(salarioBase * 0.08m, 2);

        c.AdicionarItem(TipoRubrica.SalarioBase,       "Salário Base",                    salarioBase);
        c.AdicionarItem(TipoRubrica.DescontoInss,      $"INSS ({ObterAliquotaDesc(salarioBase)})", inss);

        if (irrf > 0)
            c.AdicionarItem(TipoRubrica.DescontoIrrf,  "IRRF",                            irrf);

        c.AdicionarItem(TipoRubrica.FgtsInformativo,   "FGTS (8% — encargo patronal)",    fgts);

        return c;
    }

    private static AlertaAnomalia CriarAlerta(
        Guid empresaId, Guid funcionarioId, TipoAnomalia tipo, string descricao, int gravidade)
    {
        return AlertaAnomalia.Criar(empresaId, funcionarioId, tipo,
            new DateOnly(2026, 2, 15), descricao, gravidade);
    }

    // ─── Cálculo INSS progressivo 2024 (tabela completa) ─────────────────────
    private static decimal CalcularInssSimples(decimal salario)
    {
        var resultado = 0m;
        var faixas = new (decimal Teto, decimal Aliquota)[]
        {
            (1_412.00m, 0.075m),
            (2_666.68m, 0.09m),
            (4_000.03m, 0.12m),
            (7_786.02m, 0.14m),
        };

        var anterior = 0m;
        foreach (var (teto, aliquota) in faixas)
        {
            if (salario <= anterior) break;
            var base_ = Math.Min(salario, teto) - anterior;
            resultado += Math.Round(base_ * aliquota, 2);
            anterior = teto;
        }

        return Math.Min(resultado, 908.86m); // teto INSS 2024
    }

    private static decimal CalcularIrrfSimples(decimal baseCalculo)
    {
        return baseCalculo switch
        {
            <= 2_824.00m  => 0m,
            <= 3_751.05m  => Math.Round(baseCalculo * 0.075m - 211.80m, 2),
            <= 4_664.68m  => Math.Round(baseCalculo * 0.15m  - 492.87m, 2),
            <= 6_101.06m  => Math.Round(baseCalculo * 0.225m - 840.47m, 2),
            _             => Math.Round(baseCalculo * 0.275m - 1_145.80m, 2),
        };
    }

    private static string ObterAliquotaDesc(decimal salario) =>
        salario switch
        {
            <= 1_412.00m => "7,5%",
            <= 2_666.68m => "9%",
            <= 4_000.03m => "12%",
            <= 7_786.02m => "14%",
            _            => "Teto"
        };

    // ─── Helper de reflexão — escreve em propriedades com setter privado ──────
    private static void SetPrivateProp<T>(T obj, string propName, object? value)
    {
        var prop = typeof(T).GetProperty(propName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException($"Propriedade '{propName}' não encontrada em {typeof(T).Name}.");

        prop.SetValue(obj, value);
    }
}
