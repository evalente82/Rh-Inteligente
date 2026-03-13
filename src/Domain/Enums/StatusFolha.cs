namespace Domain.Enums;

/// <summary>
/// Ciclo de vida de um fechamento de folha de ponto.
/// </summary>
public enum StatusFolha
{
    /// <summary>Período ainda aberto — registros podem ser alterados.</summary>
    Aberta = 1,

    /// <summary>Gestor solicitou o fechamento — cálculos consolidados, aguardando aprovação.</summary>
    Fechada = 2,

    /// <summary>Dono/Gestor aprovou — folha imutável, pronta para envio à contabilidade.</summary>
    Aprovada = 3
}
