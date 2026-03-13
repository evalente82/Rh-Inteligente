namespace Domain.ValueObjects;

/// <summary>
/// Value Object imutável que representa o endereço de um colaborador.
/// Usado na entidade Admissao para fins de vínculo empregatício (CTPS/eSocial).
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed record Endereco(
    string Logradouro,
    string Numero,
    string Bairro,
    string Cidade,
    string Uf,
    string Cep,
    string? Complemento = null)
{
    // Validações no construtor canônico (Positional Record)
    public string Logradouro { get; } = ValidarCampo(Logradouro, nameof(Logradouro));
    public string Numero { get; }     = ValidarCampo(Numero, nameof(Numero));
    public string Bairro { get; }     = ValidarCampo(Bairro, nameof(Bairro));
    public string Cidade { get; }     = ValidarCampo(Cidade, nameof(Cidade));
    public string Uf { get; }         = ValidarUf(Uf);
    public string Cep { get; }        = ValidarCep(Cep);
    public string? Complemento { get; } = Complemento?.Trim();

    private static string ValidarCampo(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException($"{campo} não pode ser vazio.", campo);
        return valor.Trim();
    }

    private static string ValidarUf(string uf)
    {
        var valor = uf?.Trim().ToUpperInvariant() ?? string.Empty;
        if (valor.Length != 2)
            throw new ArgumentException("UF deve ter exatamente 2 caracteres.", nameof(uf));
        return valor;
    }

    private static string ValidarCep(string cep)
    {
        var apenasDigitos = new string(cep.Where(char.IsDigit).ToArray());
        if (apenasDigitos.Length != 8)
            throw new ArgumentException("CEP deve conter 8 dígitos.", nameof(cep));
        return apenasDigitos;
    }

    /// <summary>CEP formatado para exibição: 00000-000.</summary>
    public string CepFormatado => $"{Cep[..5]}-{Cep[5..]}";

    public override string ToString() =>
        $"{Logradouro}, {Numero}{(Complemento is not null ? $", {Complemento}" : "")} — {Bairro}, {Cidade}/{Uf} — {CepFormatado}";
}
