namespace Domain.ValueObjects;

/// <summary>
/// Value Object que representa um endereço de e-mail validado.
/// POCO puro — sem dependências externas.
/// </summary>
public sealed record Email
{
    public string Endereco { get; }

    private Email(string endereco) => Endereco = endereco;

    /// <summary>
    /// Cria um Email validado. Lança ArgumentException se o formato for inválido.
    /// </summary>
    public static Email Criar(string endereco)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endereco);

        var normalizado = endereco.Trim().ToLowerInvariant();

        // Validação simples mas suficiente: contém '@' e pelo menos um '.' após ele
        var arroba = normalizado.IndexOf('@');
        if (arroba < 1)
            throw new ArgumentException("E-mail inválido: ausência de '@'.", nameof(endereco));

        var dominio = normalizado[(arroba + 1)..];
        if (!dominio.Contains('.') || dominio.StartsWith('.') || dominio.EndsWith('.'))
            throw new ArgumentException("E-mail inválido: domínio malformado.", nameof(endereco));

        return new Email(normalizado);
    }

    /// <summary>
    /// Tenta criar um Email sem lançar exceção.
    /// Retorna null se o formato for inválido.
    /// </summary>
    public static Email? TentarCriar(string endereco)
    {
        try { return Criar(endereco); }
        catch { return null; }
    }

    public override string ToString() => Endereco;
}
