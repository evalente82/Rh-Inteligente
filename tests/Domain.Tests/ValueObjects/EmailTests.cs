using Domain.ValueObjects;

namespace Domain.Tests.ValueObjects;

public sealed class EmailTests
{
    [Theory]
    [InlineData("gestor@vcorp.com.br")]
    [InlineData("GESTOR@VCORP.COM.BR")]  // normaliza para lowercase
    [InlineData("user.name+tag@domain.co")]
    public void Criar_EmailValido_RetornaEmailNormalizado(string email)
    {
        var emailVo = Email.Criar(email);
        Assert.Equal(email.ToLowerInvariant().Trim(), emailVo.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("semArroba")]
    [InlineData("@sem-usuario")]
    [InlineData("usuario@")]
    [InlineData("usuario@.com")]
    [InlineData("usuario@dominio.")]
    public void Criar_EmailInvalido_LancaArgumentException(string emailInvalido)
    {
        Assert.Throws<ArgumentException>(() => Email.Criar(emailInvalido));
    }

    [Fact]
    public void TentarCriar_EmailInvalido_RetornaNull()
    {
        var resultado = Email.TentarCriar("invalido");
        Assert.Null(resultado);
    }

    [Fact]
    public void TentarCriar_EmailValido_RetornaEmail()
    {
        var resultado = Email.TentarCriar("ok@ok.com");
        Assert.NotNull(resultado);
        Assert.Equal("ok@ok.com", resultado!.Endereco);
    }

    [Fact]
    public void ToString_RetornaEndereco()
    {
        var email = Email.Criar("teste@empresa.com.br");
        Assert.Equal("teste@empresa.com.br", email.ToString());
    }
}
