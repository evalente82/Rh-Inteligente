using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Obtém um contracheque específico pelo seu identificador.
/// </summary>
public sealed class ObterContrachequeUseCase
{
    private readonly IContrachequeRepository _contrachequeRepository;

    public ObterContrachequeUseCase(IContrachequeRepository contrachequeRepository)
    {
        _contrachequeRepository = contrachequeRepository;
    }

    public async Task<ContrachequeOutputDTO> ExecutarAsync(Guid id, CancellationToken ct = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id do contracheque não pode ser vazio.", nameof(id));

        var contracheque = await _contrachequeRepository.ObterPorIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Contracheque '{id}' não encontrado.");

        return GerarContrachequeUseCase.ToOutputDTO(contracheque);
    }
}
