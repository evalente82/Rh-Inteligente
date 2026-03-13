using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.UseCases;

/// <summary>
/// Registra um novo usuário em um tenant existente.
/// Somente um Dono/Gestor deve chamar este endpoint (protegido por [Authorize(Roles=Dono,Gestor)]).
/// </summary>
public sealed class RegistrarUsuarioUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrarUsuarioUseCase(
        IEmpresaRepository empresaRepository,
        IUsuarioRepository usuarioRepository,
        ISenhaHasher senhaHasher,
        IUnitOfWork unitOfWork)
    {
        _empresaRepository = empresaRepository;
        _usuarioRepository = usuarioRepository;
        _senhaHasher = senhaHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> ExecutarAsync(RegistrarUsuarioInputDTO input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Garante que a empresa existe
        var empresa = await _empresaRepository.ObterPorIdAsync(input.EmpresaId, ct)
            ?? throw new KeyNotFoundException($"Empresa {input.EmpresaId} não encontrada.");

        if (!empresa.Ativa)
            throw new InvalidOperationException("Empresa inativa. Não é possível adicionar usuários.");

        // E-mail único dentro do tenant
        var existente = await _usuarioRepository.ObterPorEmailAsync(input.EmpresaId, input.Email, ct);
        if (existente is not null)
            throw new InvalidOperationException($"E-mail '{input.Email}' já está em uso neste tenant.");

        var senhaHash = _senhaHasher.Hashear(input.Senha);
        var usuario = Usuario.Criar(input.EmpresaId, input.Email, senhaHash, input.NomeCompleto, input.Role);

        await _usuarioRepository.AdicionarAsync(usuario, ct);
        await _unitOfWork.CommitAsync(ct);

        return usuario.Id;
    }
}
