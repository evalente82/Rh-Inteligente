namespace Application.DTOs;

/// <summary>Dados necessários para onboarding de uma nova empresa no SaaS.</summary>
public sealed record CriarEmpresaInputDTO(
    string NomeFantasia,
    string Cnpj,
    string EmailContato,
    /// <summary>Nome do usuário Dono que será criado junto com a empresa.</summary>
    string NomeDonoUsuario,
    string SenhaDono
);
