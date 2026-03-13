using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class RegistrarUsuarioUseCaseTests
{
    private readonly Mock<IEmpresaRepository> _empresaRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<ISenhaHasher> _senhaHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private RegistrarUsuarioUseCase CriarUseCase() =>
        new(_empresaRepo.Object, _usuarioRepo.Object, _senhaHasher.Object, _unitOfWork.Object);

    private static Empresa EmpresaAtiva() =>
        Empresa.Criar("Vcorp Ltda", "12345678000195", "dono@vcorp.com.br");

    [Fact]
    public async Task ExecutarAsync_DadosValidos_CriaUsuarioECommita()
    {
        // Arrange
        var empresa = EmpresaAtiva();
        var input = new RegistrarUsuarioInputDTO(
            empresa.Id, "Carlos Gestor", "carlos@vcorp.com.br", "Senha@123", Role.Gestor);

        _empresaRepo.Setup(r => r.ObterPorIdAsync(empresa.Id, default)).ReturnsAsync(empresa);
        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresa.Id, "carlos@vcorp.com.br", default))
            .ReturnsAsync((Usuario?)null);
        _senhaHasher.Setup(h => h.Hashear("Senha@123")).Returns("hash_bcrypt");

        // Act
        var usuarioId = await CriarUseCase().ExecutarAsync(input);

        // Assert
        Assert.NotEqual(Guid.Empty, usuarioId);
        _usuarioRepo.Verify(r => r.AdicionarAsync(
            It.Is<Usuario>(u => u.Role == Role.Gestor && u.EmpresaId == empresa.Id),
            default), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_EmpresaNaoEncontrada_LancaKeyNotFoundException()
    {
        var input = new RegistrarUsuarioInputDTO(
            Guid.NewGuid(), "X", "x@x.com", "s", Role.Colaborador);

        _empresaRepo.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((Empresa?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CriarUseCase().ExecutarAsync(input));

        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_EmailDuplicado_LancaInvalidOperationException()
    {
        // Arrange
        var empresa = EmpresaAtiva();
        var input = new RegistrarUsuarioInputDTO(
            empresa.Id, "Outro", "carlos@vcorp.com.br", "Senha@123", Role.Colaborador);

        _empresaRepo.Setup(r => r.ObterPorIdAsync(empresa.Id, default)).ReturnsAsync(empresa);

        // Simula usuário já existente com mesmo e-mail
        var usuarioExistente = Usuario.Criar(empresa.Id, "carlos@vcorp.com.br", "hash", "Carlos", Role.Gestor);
        _usuarioRepo.Setup(r => r.ObterPorEmailAsync(empresa.Id, "carlos@vcorp.com.br", default))
            .ReturnsAsync(usuarioExistente);

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
