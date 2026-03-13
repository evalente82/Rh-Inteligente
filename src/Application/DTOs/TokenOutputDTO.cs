namespace Application.DTOs;

/// <summary>Resposta de autenticação com tokens JWT.</summary>
public sealed record TokenOutputDTO(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiraEm,
    Guid UsuarioId,
    Guid EmpresaId,
    string NomeCompleto,
    string Role
);
