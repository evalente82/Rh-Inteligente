namespace Domain.Enums;

/// <summary>
/// Nível de acesso do usuário dentro de uma empresa (tenant).
/// </summary>
public enum Role
{
    /// <summary>Proprietário da conta — acesso total ao SaaS.</summary>
    Dono = 1,

    /// <summary>Gestor de RH/DP — acesso operacional a ponto, folha e CCT.</summary>
    Gestor = 2,

    /// <summary>Colaborador — acesso somente às suas próprias marcações.</summary>
    Colaborador = 3
}
