using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class LoginUseCaseTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<ISenhaHasher> _senhaHasher = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private LoginUseCase CriarUseCase() =>
        new(_usuarioRepo.Object, _senhaHasher.Object, _jwtService.Object, _unitOfWork.Object);

    private static Usuario UsuarioAtivo(Guid empresaId) =>
        Usuario.Criar(empresaId, "gestor@vcorp.com.br", "hash_bcrypt", "Carlos Gestor", Role.Gestor);

    [Fact]
    public async Task ExecutarAsync_CredenciaisValidas_RetornaToken()
    {
        // Arrange
        var empresaId = Guid.NewGuid();
        var usuario = UsuarioAtivo(empresaId);
        var input = new LoginInputDTO(empresaId, "gestor@vcorp.com.br", "Senha@123");

        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresaId, "gestor@vcorp.com.br", default))
            .ReturnsAsync(usuario);
        _senhaHasher.Setup(h => h.Verificar("Senha@123", "hash_bcrypt")).Returns(true);
        _jwtService.Setup(j => j.GerarAccessToken(usuario)).Returns("access_jwt_token");
        _jwtService.Setup(j => j.GerarRefreshToken()).Returns("refresh_token_opaco");

        // Act
        var token = await CriarUseCase().ExecutarAsync(input);

        // Assert
        Assert.Equal("access_jwt_token", token.AccessToken);
        Assert.Equal("refresh_token_opaco", token.RefreshToken);
        Assert.Equal(usuario.Id, token.UsuarioId);
        Assert.Equal(empresaId, token.EmpresaId);
        Assert.Equal("Gestor", token.Role);

        _usuarioRepo.Verify(r => r.Atualizar(It.IsAny<Usuario>()), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_EmailNaoEncontrado_LancaKeyNotFoundException()
    {
        var empresaId = Guid.NewGuid();
        var input = new LoginInputDTO(empresaId, "inexistente@vcorp.com.br", "senha");

        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresaId, "inexistente@vcorp.com.br", default))
            .ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CriarUseCase().ExecutarAsync(input));

        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_SenhaErrada_LancaKeyNotFoundException()
    {
        var empresaId = Guid.NewGuid();
        var usuario = UsuarioAtivo(empresaId);
        var input = new LoginInputDTO(empresaId, "gestor@vcorp.com.br", "SenhaErrada");

        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresaId, "gestor@vcorp.com.br", default))
            .ReturnsAsync(usuario);
        _senhaHasher.Setup(h => h.Verificar("SenhaErrada", "hash_bcrypt")).Returns(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CriarUseCase().ExecutarAsync(input));
    }

    [Fact]
    public async Task ExecutarAsync_UsuarioDesativado_LancaInvalidOperationException()
    {
        var empresaId = Guid.NewGuid();
        var usuario = UsuarioAtivo(empresaId);
        usuario.Desativar(); // desativa antes de tentar logar
        var input = new LoginInputDTO(empresaId, "gestor@vcorp.com.br", "Senha@123");

        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresaId, "gestor@vcorp.com.br", default))
            .ReturnsAsync(usuario);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CriarUseCase().ExecutarAsync(input));
    }

    [Fact]
    public async Task ExecutarAsync_InputNulo_LancaArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CriarUseCase().ExecutarAsync(null!));
    }
}
