namespace Domain.Enums;

/// <summary>
/// Regime de contratação do colaborador conforme legislação brasileira.
/// </summary>
public enum RegimeContratacao
{
    /// <summary>Consolidação das Leis do Trabalho — vínculo empregatício padrão.</summary>
    Clt = 1,

    /// <summary>Pessoa Jurídica — prestador de serviço sem vínculo empregatício.</summary>
    Pj = 2,

    /// <summary>Estágio supervisionado (Lei 11.788/2008).</summary>
    Estagio = 3,

    /// <summary>Contrato temporário (Lei 6.019/1974).</summary>
    Temporario = 4
}
