using Infrastructure.AI;

namespace Domain.Tests.UseCases;

/// <summary>
/// Testes unitários para a lógica de parse JSON do GeminiAuditorIaService.
/// Usa o parser público exposto via InfrastructureTestHelpers.
/// </summary>
public sealed class GeminiAuditorIaServiceJsonParserTests
{
    private static readonly Guid EmpresaId = Guid.Parse("33333333-0000-0000-0000-000000000001");
    private static readonly Guid FuncionarioId = Guid.Parse("33333333-0000-0000-0000-000000000002");

    private static Funcionario CriarFuncionario() =>
        Funcionario.Criar(
            EmpresaId, "Teste Parser",
            new Cpf("529.982.247-25"), "F300",
            new DateTime(2025, 1, 1),
            new TurnoTrabalho(
                new TimeOnly(8, 0),
                new TimeOnly(17, 0),
                TimeSpan.FromHours(1)));

    [Fact]
    public void ParsearJson_ArrayVazio_DeveRetornarListaVazia()
    {
        var funcionario = CriarFuncionario();
        var resultado = AnomaliaJsonParser.Parsear("[]", funcionario).ToList();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public void ParsearJson_JsonComMarkdown_DeveRemoverMarkdownEParsear()
    {
        var funcionario = CriarFuncionario();
        var json = """
            ```json
            []
            ```
            """;

        var resultado = AnomaliaJsonParser.Parsear(json, funcionario).ToList();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public void ParsearJson_JsonMalformado_DeveRetornarListaVazia()
    {
        var funcionario = CriarFuncionario();
        var resultado = AnomaliaJsonParser.Parsear("ERRO: texto inválido", funcionario).ToList();
        resultado.Should().BeEmpty();
    }

    [Fact]
    public void ParsearJson_UmaAnomalia_DeveRetornarUmAlerta()
    {
        var funcionario = CriarFuncionario();
        var json = """
            [
              {
                "tipoAnomalia": "IntervaloInsuficiente",
                "descricao": "Intervalo de apenas 30 minutos (mínimo: 60).",
                "gravidade": 3,
                "dataOcorrencia": "2026-03-05T12:00:00",
                "minutosAnomalia": 30
              }
            ]
            """;

        var resultado = AnomaliaJsonParser.Parsear(json, funcionario).ToList();

        resultado.Should().HaveCount(1);
        resultado[0].TipoAnomalia.Should().Be(TipoAnomalia.IntervaloInsuficiente);
        resultado[0].Descricao.Should().Contain("Intervalo");
        resultado[0].Gravidade.Should().Be(3);
        resultado[0].EmpresaId.Should().Be(EmpresaId);
    }

    [Fact]
    public void ParsearJson_MultiplosTipos_DeveMapearCorretamente()
    {
        var funcionario = CriarFuncionario();
        var json = """
            [
              { "tipoAnomalia": "HoraExtraExcessiva",    "descricao": "Hora extra.",     "gravidade": 2, "dataOcorrencia": "2026-03-01T00:00:00", "minutosAnomalia": 60 },
              { "tipoAnomalia": "FaltaInjustificada",    "descricao": "Falta.",          "gravidade": 3, "dataOcorrencia": "2026-03-02T00:00:00", "minutosAnomalia": 0  },
              { "tipoAnomalia": "RiscoDeDobra",          "descricao": "Risco de dobra.", "gravidade": 2, "dataOcorrencia": "2026-03-03T00:00:00", "minutosAnomalia": 0  },
              { "tipoAnomalia": "BatidaForaDeSequencia", "descricao": "Seq. inválida.",  "gravidade": 1, "dataOcorrencia": "2026-03-04T00:00:00", "minutosAnomalia": 0  }
            ]
            """;

        var resultado = AnomaliaJsonParser.Parsear(json, funcionario).ToList();

        resultado.Should().HaveCount(4);
        resultado[0].TipoAnomalia.Should().Be(TipoAnomalia.HoraExtraInesperada);
        resultado[1].TipoAnomalia.Should().Be(TipoAnomalia.FaltaDeRegistro);
        resultado[2].TipoAnomalia.Should().Be(TipoAnomalia.RiscoDeDobra);
        resultado[3].TipoAnomalia.Should().Be(TipoAnomalia.BatidaForaDeSequencia);
    }

    [Fact]
    public void ParsearJson_GravidadeFora_DeveSerClampada()
    {
        var funcionario = CriarFuncionario();
        var json = """
            [
              { "tipoAnomalia": "AtrasoEntrada", "descricao": "Atraso.", "gravidade": 99,
                "dataOcorrencia": "2026-03-10T08:30:00", "minutosAnomalia": 30 }
            ]
            """;

        var resultado = AnomaliaJsonParser.Parsear(json, funcionario).ToList();

        resultado.Should().HaveCount(1);
        resultado[0].Gravidade.Should().BeLessOrEqualTo(3);
    }
}
