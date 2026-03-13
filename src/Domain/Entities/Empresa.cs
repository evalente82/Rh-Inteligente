namespace Domain.Entities;

/// <summary>
/// Agregado raiz que representa uma empresa (tenant) cadastrada no SaaS.
/// POCO puro — sem dependências externas.
/// </summary>
public sealed class Empresa
{
    public Guid Id { get; private set; }

    /// <summary>Razão social ou nome fantasia da empresa.</summary>
    public string NomeFantasia { get; private set; } = string.Empty;

    /// <summary>CNPJ da empresa (14 dígitos, sem formatação).</summary>
    public string Cnpj { get; private set; } = string.Empty;

    /// <summary>E-mail de contato/cobrança da conta SaaS.</summary>
    public string EmailContato { get; private set; } = string.Empty;

    public DateTime CriadaEm { get; private set; }

    public bool Ativa { get; private set; }

    private Empresa() { }

    /// <summary>
    /// Cria uma nova empresa validando as invariantes do domínio.
    /// </summary>
    public static Empresa Criar(string nomeFantasia, string cnpj, string emailContato)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nomeFantasia);
        ArgumentException.ThrowIfNullOrWhiteSpace(cnpj);
        ArgumentException.ThrowIfNullOrWhiteSpace(emailContato);

        var cnpjSemFormatacao = cnpj.Replace(".", "").Replace("/", "").Replace("-", "").Trim();
        if (cnpjSemFormatacao.Length != 14)
            throw new ArgumentException("CNPJ deve conter 14 dígitos.", nameof(cnpj));

        if (!cnpjSemFormatacao.All(char.IsDigit))
            throw new ArgumentException("CNPJ deve conter apenas dígitos.", nameof(cnpj));

        // Valida o VO Email (reutiliza lógica de validação)
        ValueObjects.Email.Criar(emailContato);

        return new Empresa
        {
            Id = Guid.NewGuid(),
            NomeFantasia = nomeFantasia.Trim(),
            Cnpj = cnpjSemFormatacao,
            EmailContato = emailContato.Trim().ToLowerInvariant(),
            CriadaEm = DateTime.UtcNow,
            Ativa = true
        };
    }

    /// <summary>Desativa a empresa impedindo novos acessos ao tenant.</summary>
    public void Desativar()
    {
        if (!Ativa)
            throw new InvalidOperationException("Empresa já está desativada.");

        Ativa = false;
    }

    /// <summary>Formata o CNPJ no padrão 00.000.000/0000-00.</summary>
    public string CnpjFormatado =>
        $"{Cnpj[..2]}.{Cnpj[2..5]}.{Cnpj[5..8]}/{Cnpj[8..12]}-{Cnpj[12..14]}";
}
