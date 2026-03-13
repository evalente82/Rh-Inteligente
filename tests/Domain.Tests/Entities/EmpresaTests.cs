using Domain.Entities;

namespace Domain.Tests.Entities;

public sealed class EmpresaTests
{
    [Fact]
    public void Criar_DadosValidos_RetornaEmpresaAtiva()
    {
        var empresa = Empresa.Criar("Vcorp Ltda", "12345678000195", "contato@vcorp.com.br");

        Assert.NotEqual(Guid.Empty, empresa.Id);
        Assert.Equal("Vcorp Ltda", empresa.NomeFantasia);
        Assert.Equal("12345678000195", empresa.Cnpj);
        Assert.Equal("12.345.678/0001-95", empresa.CnpjFormatado);
        Assert.Equal("contato@vcorp.com.br", empresa.EmailContato);
        Assert.True(empresa.Ativa);
    }

    [Theory]
    [InlineData("1234567800019")]   // 13 dígitos
    [InlineData("123456780001950")] // 15 dígitos
    [InlineData("1234567800019A")]  // letra
    public void Criar_CnpjInvalido_LancaArgumentException(string cnpjInvalido)
    {
        Assert.Throws<ArgumentException>(() =>
            Empresa.Criar("Vcorp", cnpjInvalido, "ok@ok.com"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_NomeFantasiaVazio_LancaArgumentException(string nome)
    {
        Assert.Throws<ArgumentException>(() =>
            Empresa.Criar(nome, "12345678000195", "ok@ok.com"));
    }

    [Fact]
    public void Desativar_EmpresaAtiva_MudaParaInativa()
    {
        var empresa = Empresa.Criar("Vcorp", "12345678000195", "ok@ok.com");
        empresa.Desativar();
        Assert.False(empresa.Ativa);
    }

    [Fact]
    public void Desativar_EmpresaJaInativa_LancaInvalidOperationException()
    {
        var empresa = Empresa.Criar("Vcorp", "12345678000195", "ok@ok.com");
        empresa.Desativar();
        Assert.Throws<InvalidOperationException>(() => empresa.Desativar());
    }
}
