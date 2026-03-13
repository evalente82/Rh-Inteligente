using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.UseCases;

/// <summary>
/// Onboarding de um novo tenant: cria a Empresa e o usuário Dono atomicamente.
/// </summary>
public sealed class CriarEmpresaUseCase
{
    private readonly IEmpresaRepository _empresaRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISenhaHasher _senhaHasher;
    private readonly IUnitOfWork _unitOfWork;

    public CriarEmpresaUseCase(
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

    public async Task<EmpresaOutputDTO> ExecutarAsync(CriarEmpresaInputDTO input, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Garante CNPJ único no sistema
        var cnpjLimpo = input.Cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        var existente = await _empresaRepository.ObterPorCnpjAsync(cnpjLimpo, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe uma empresa cadastrada com o CNPJ {cnpjLimpo}.");

        // Cria o agregado Empresa (valida CNPJ e e-mail no domínio)
        var empresa = Empresa.Criar(input.NomeFantasia, input.Cnpj, input.EmailContato);

        // Cria o usuário Dono vinculado ao tenant recém-criado
        var senhaHash = _senhaHasher.Hashear(input.SenhaDono);
        var dono = Usuario.Criar(empresa.Id, input.EmailContato, senhaHash, input.NomeDonoUsuario, Role.Dono);

        await _empresaRepository.AdicionarAsync(empresa, ct);
        await _usuarioRepository.AdicionarAsync(dono, ct);
        await _unitOfWork.CommitAsync(ct);

        return new EmpresaOutputDTO(
            empresa.Id,
            empresa.NomeFantasia,
            empresa.CnpjFormatado,
            empresa.EmailContato,
            empresa.CriadaEm);
    }
}
