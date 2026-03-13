using Domain.Enums;

namespace Application.DTOs;

/// <summary>Dados para registrar um novo usuário em um tenant existente.</summary>
public sealed record RegistrarUsuarioInputDTO(
    Guid EmpresaId,
    string NomeCompleto,
    string Email,
    string Senha,
    Role Role
);
