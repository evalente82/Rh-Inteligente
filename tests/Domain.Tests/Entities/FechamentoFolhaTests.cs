using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities;

public sealed class FechamentoFolhaTests
{
    [Fact]
    public void Abrir_DadosValidos_CriaFolhaAberta()
    {
        var empresaId = Guid.NewGuid();
        var inicio = new DateOnly(2026, 3, 1);
        var fim = new DateOnly(2026, 3, 31);

        var folha = FechamentoFolha.Abrir(empresaId, inicio, fim);

        Assert.Equal(StatusFolha.Aberta, folha.Status);
        Assert.Equal(empresaId, folha.EmpresaId);
        Assert.Equal(inicio, folha.PeriodoInicio);
        Assert.Equal(fim, folha.PeriodoFim);
        Assert.Null(folha.FechadaEm);
        Assert.Null(folha.RelatorioNarrativo);
    }

    [Fact]
    public void Abrir_PeriodoFimIgualInicio_LancaArgumentException()
    {
        var data = new DateOnly(2026, 3, 1);
        Assert.Throws<ArgumentException>(
            () => FechamentoFolha.Abrir(Guid.NewGuid(), data, data));
    }

    [Fact]
    public void Fechar_FolhaAberta_TransitaParaFechada()
    {
        var folha = FechamentoFolha.Abrir(
            Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        folha.Fechar(10.5m, 1.0m, 2, "Relatório de teste.");

        Assert.Equal(StatusFolha.Fechada, folha.Status);
        Assert.Equal(10.5m, folha.TotalHorasExtras);
        Assert.Equal(1.0m, folha.TotalDescontos);
        Assert.Equal(2, folha.TotalAnomaliasCriticas);
        Assert.NotNull(folha.FechadaEm);
        Assert.Equal("Relatório de teste.", folha.RelatorioNarrativo);
    }

    [Fact]
    public void Fechar_FolhaJaFechada_LancaInvalidOperationException()
    {
        var folha = FechamentoFolha.Abrir(
            Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        folha.Fechar(0, 0, 0, "Narrativa.");

        Assert.Throws<InvalidOperationException>(() =>
            folha.Fechar(0, 0, 0, "Segunda vez."));
    }

    [Fact]
    public void Aprovar_FolhaFechada_TransitaParaAprovada()
    {
        var folha = FechamentoFolha.Abrir(
            Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        folha.Fechar(5m, 0m, 0, "Relatório.");

        folha.Aprovar();

        Assert.Equal(StatusFolha.Aprovada, folha.Status);
        Assert.NotNull(folha.AprovadaEm);
    }

    [Fact]
    public void Aprovar_FolhaAberta_LancaInvalidOperationException()
    {
        var folha = FechamentoFolha.Abrir(
            Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));

        Assert.Throws<InvalidOperationException>(() => folha.Aprovar());
    }

    [Fact]
    public void AtualizarRelatorio_FolhaAprovada_LancaInvalidOperationException()
    {
        var folha = FechamentoFolha.Abrir(
            Guid.NewGuid(), new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        folha.Fechar(0, 0, 0, "Original.");
        folha.Aprovar();

        Assert.Throws<InvalidOperationException>(
            () => folha.AtualizarRelatorio("Nova narrativa."));
    }
}
