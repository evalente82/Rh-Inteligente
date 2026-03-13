using Domain.Entities;
using Domain.Enums;

namespace Domain.Tests.Entities;

public sealed class UsuarioTests
{
    private static readonly Guid EmpresaIdValido = Guid.NewGuid();

    [Fact]
    public void Criar_DadosValidos_RetornaUsuarioAtivo()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "gestor@vcorp.com.br", "hash", "Carlos", Role.Gestor);

        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal(EmpresaIdValido, usuario.EmpresaId);
        Assert.Equal("gestor@vcorp.com.br", usuario.Email.Endereco);
        Assert.Equal("Carlos", usuario.NomeCompleto);
        Assert.Equal(Role.Gestor, usuario.Role);
        Assert.True(usuario.Ativo);
        Assert.Null(usuario.RefreshToken);
    }

    [Fact]
    public void Criar_EmpresaIdVazio_LancaArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Usuario.Criar(Guid.Empty, "x@x.com", "hash", "X", Role.Dono));
    }

    [Fact]
    public void DefinirRefreshToken_DadosValidos_PersistirToken()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "a@a.com", "h", "Ana", Role.Colaborador);
        var expiracao = DateTime.UtcNow.AddDays(7);

        usuario.DefinirRefreshToken("meu_token_opaco", expiracao);

        Assert.Equal("meu_token_opaco", usuario.RefreshToken);
        Assert.Equal(expiracao, usuario.RefreshTokenExpiracao);
    }

    [Fact]
    public void DefinirRefreshToken_ExpiracaoPassada_LancaArgumentException()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "a@a.com", "h", "Ana", Role.Colaborador);

        Assert.Throws<ArgumentException>(() =>
            usuario.DefinirRefreshToken("token", DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void RevogarRefreshToken_LimpaTokenEExpiracao()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "a@a.com", "h", "Ana", Role.Colaborador);
        usuario.DefinirRefreshToken("token", DateTime.UtcNow.AddDays(7));
        usuario.RevogarRefreshToken();

        Assert.Null(usuario.RefreshToken);
        Assert.Null(usuario.RefreshTokenExpiracao);
    }

    [Fact]
    public void Desativar_UsuarioAtivo_MudaParaInativo()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "a@a.com", "h", "Ana", Role.Colaborador);
        usuario.Desativar();
        Assert.False(usuario.Ativo);
        Assert.Null(usuario.RefreshToken);
    }

    [Fact]
    public void Desativar_UsuarioJaInativo_LancaInvalidOperationException()
    {
        var usuario = Usuario.Criar(EmpresaIdValido, "a@a.com", "h", "Ana", Role.Colaborador);
        usuario.Desativar();
        Assert.Throws<InvalidOperationException>(() => usuario.Desativar());
    }
}
