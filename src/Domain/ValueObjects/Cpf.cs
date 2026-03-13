namespace Domain.ValueObjects;

/// <summary>
/// Value Object imutável que representa e valida um CPF brasileiro.
/// Contém a lógica de verificação dos dois dígitos verificadores (algoritmo Módulo 11).
/// POCO puro — sem dependências externas (Regra 2).
/// </summary>
public sealed record Cpf
{
    /// <summary>CPF armazenado apenas como 11 dígitos numéricos, sem formatação.</summary>
    public string Numero { get; }

    /// <summary>CPF formatado para exibição: 000.000.000-00.</summary>
    public string NumeroFormatado => $"{Numero[..3]}.{Numero[3..6]}.{Numero[6..9]}-{Numero[9..]}";

    public Cpf(string numero)
    {
        var apenasDigitos = new string(numero.Where(char.IsDigit).ToArray());

        if (apenasDigitos.Length != 11)
            throw new ArgumentException("CPF deve conter exatamente 11 dígitos.", nameof(numero));

        if (!ValidarDigitos(apenasDigitos))
            throw new ArgumentException($"CPF '{numero}' é inválido.", nameof(numero));

        Numero = apenasDigitos;
    }

    // -------------------------------------------------------------------------
    // Algoritmo de validação — Módulo 11
    // -------------------------------------------------------------------------

    private static bool ValidarDigitos(string cpf)
    {
        // Rejeita sequências repetidas (ex: 111.111.111-11)
        if (cpf.Distinct().Count() == 1) return false;

        return ValidarDigito(cpf, 9) && ValidarDigito(cpf, 10);
    }

    private static bool ValidarDigito(string cpf, int posicao)
    {
        var soma = 0;
        var multiplicador = posicao + 1;

        for (var i = 0; i < posicao; i++)
        {
            soma += int.Parse(cpf[i].ToString()) * multiplicador--;
        }

        var resto = (soma * 10) % 11;
        var digito = resto == 10 ? 0 : resto;

        return digito == int.Parse(cpf[posicao].ToString());
    }

    // Permite comparação e uso como chave sem quebrar o record
    public override string ToString() => NumeroFormatado;

    /// <summary>Tenta criar um CPF, retornando null se inválido (sem lançar exceção).</summary>
    public static Cpf? TentarCriar(string numero)
    {
        try { return new Cpf(numero); }
        catch (ArgumentException) { return null; }
    }
}
