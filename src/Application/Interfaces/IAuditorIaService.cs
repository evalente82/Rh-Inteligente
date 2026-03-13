using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// Contrato do serviço de IA que audita os registros de ponto e gera alertas de anomalia.
/// Implementado na camada Infrastructure com integração ao LLM (ex: OpenAI, Gemini).
/// A chamada ao LLM ocorre em background job — nunca de forma síncrona na API.
/// </summary>
public interface IAuditorIaService
{
    /// <summary>
    /// Analisa os registros de um funcionário em um determinado período e retorna
    /// uma lista de anomalias identificadas pela IA.
    /// </summary>
    Task<IEnumerable<AlertaAnomalia>> AnalisarAsync(
        Funcionario funcionario,
        IEnumerable<RegistroPonto> registros,
        CancellationToken cancellationToken = default);
}
