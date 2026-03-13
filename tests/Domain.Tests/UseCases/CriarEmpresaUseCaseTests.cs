using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Domain.Entities;
using Domain.Enums;
using Moq;

namespace Domain.Tests.UseCases;

public sealed class CriarEmpresaUseCaseTests
{
    private readonly Mock<IEmpresaRepository> _empresaRepo = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepo = new();
    private readonly Mock<ISenhaHasher> _senhaHasher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private CriarEmpresaUseCase CriarUseCase() =>
        new(_empresaRepo.Object, _usuarioRepo.Object, _senhaHasher.Object, _unitOfWork.Object);

    [Fact]
    public async Task ExecutarAsync_CnpjNovo_CriaEmpresaEUsuarioDono()
    {
        // Arrange
        var input = new CriarEmpresaInputDTO(
            "Vcorp Ltda", "12345678000195", "dono@vcorp.com.br",
            "João Dono", "Senha@123");

        _empresaRepo.Setup(r => r.ObterPorCnpjAsync("12345678000195", default))
            .ReturnsAsync((Empresa?)null);

        _senhaHasher.Setup(h => h.Hashear("Senha@123")).Returns("hash_bcrypt");

        // Act
        var resultado = await CriarUseCase().ExecutarAsync(input);

        // Assert
        Assert.Equal("Vcorp Ltda", resultado.NomeFantasia);
        Assert.Equal("12.345.678/0001-95", resultado.CnpjFormatado);
        Assert.Equal("dono@vcorp.com.br", resultado.EmailContato);

        _empresaRepo.Verify(r => r.AdicionarAsync(It.IsAny<Empresa>(), default), Times.Once);
        _usuarioRepo.Verify(r => r.AdicionarAsync(
            It.Is<Usuario>(u => u.Role == Role.Dono && u.EmpresaId != Guid.Empty),
            default), Times.Once);
        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_CnpjDuplicado_LancaInvalidOperationException()
    {
        // Arrange
        var input = new CriarEmpresaInputDTO(
            "Outra Empresa", "12345678000195", "outro@vcorp.com.br",
            "Maria", "Senha@123");

        _empresaRepo.Setup(r => r.ObterPorCnpjAsync("12345678000195", default))
            .ReturnsAsync(Empresa.Criar("Vcorp Ltda", "12345678000195", "dono@vcorp.com.br"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CriarUseCase().ExecutarAsync(input));

        _unitOfWork.Verify(u => u.CommitAsync(default), Times.Never);
    }

    [Fact]
    public async Task ExecutarAsync_CnpjComFormatacao_NormalizaAntesDeBuscar()
    {
        // Arrange — CNPJ formatado com pontos e traços
        var input = new CriarEmpresaInputDTO(
            "Vcorp", "12.345.678/0001-95", "teste@vcorp.com.br",
            "Ana", "Senha@123");

        _empresaRepo.Setup(r => r.ObterPorCnpjAsync("12345678000195", default))
            .ReturnsAsync((Empresa?)null);
        _senhaHasher.Setup(h => h.Hashear(It.IsAny<string>())).Returns("hash");

        // Act
        await CriarUseCase().ExecutarAsync(input);

        // Verifica que a busca foi feita com o CNPJ sem formatação
        _empresaRepo.Verify(r => r.ObterPorCnpjAsync("12345678000195", default), Times.Once);
    }

    [Fact]
    public async Task ExecutarAsync_InputNulo_LancaArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => CriarUseCase().ExecutarAsync(null!));
    }
}
