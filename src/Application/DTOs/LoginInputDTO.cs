namespace Application.DTOs;

/// <summary>Credenciais de login do usuário.</summary>
public sealed record LoginInputDTO(
    Guid EmpresaId,
    string Email,
    string Senha
);
