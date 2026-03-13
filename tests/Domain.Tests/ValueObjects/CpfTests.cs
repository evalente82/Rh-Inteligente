namespace Domain.Tests.ValueObjects;

public class CpfTests
{
    // CPFs válidos gerados por algoritmo para testes
    private const string CpfValido = "529.982.247-25";
    private const string CpfValidoSemMascara = "52998224725";

    [Fact]
    public void Criar_CpfValido_DeveInstanciar()
    {
        var cpf = new Cpf(CpfValido);
        cpf.Numero.Should().Be(CpfValidoSemMascara);
    }

    [Fact]
    public void Criar_CpfSemFormatacao_DeveInstanciar()
    {
        var cpf = new Cpf(CpfValidoSemMascara);
        cpf.Numero.Should().Be(CpfValidoSemMascara);
    }

    [Fact]
    public void NumeroFormatado_DeveRetornarComMascara()
    {
        var cpf = new Cpf(CpfValidoSemMascara);
        cpf.NumeroFormatado.Should().Be("529.982.247-25");
    }

    [Theory]
    [InlineData("111.111.111-11")]  // sequência repetida
    [InlineData("000.000.000-00")]  // sequência repetida
    [InlineData("123.456.789-00")]  // dígito verificador errado
    [InlineData("529.982.247-26")]  // último dígito errado
    public void Criar_CpfInvalido_DeveLancarArgumentException(string cpfInvalido)
    {
        var acao = () => new Cpf(cpfInvalido);
        acao.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("12345")]           // menos de 11 dígitos
    [InlineData("1234567890123")]   // mais de 11 dígitos
    [InlineData("")]
    public void Criar_CpfTamanhoErrado_DeveLancarArgumentException(string cpfInvalido)
    {
        var acao = () => new Cpf(cpfInvalido);
        acao.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TentarCriar_CpfValido_DeveRetornarInstancia()
    {
        var cpf = Cpf.TentarCriar(CpfValido);
        cpf.Should().NotBeNull();
    }

    [Fact]
    public void TentarCriar_CpfInvalido_DeveRetornarNull()
    {
        var cpf = Cpf.TentarCriar("000.000.000-00");
        cpf.Should().BeNull();
    }

    [Fact]
    public void Igualdade_DoisCpfsComMesmoNumero_DevemSerIguais()
    {
        var cpf1 = new Cpf(CpfValido);
        var cpf2 = new Cpf(CpfValidoSemMascara);
        cpf1.Should().Be(cpf2);
    }
}
