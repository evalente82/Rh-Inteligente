using Application.DTOs;
using Application.Interfaces;

namespace Application.UseCases;

/// <summary>
/// Retorna todos os contracheques de um fechamento de folha.
/// Leitura pura — não modifica estado.
/// </summary>
public sealed class ListarContrachequesFolhaUseCase
{
    private readonly IContrachequeRepository _contrachequeRepository;

    public ListarContrachequesFolhaUseCase(IContrachequeRepository contrachequeRepository)
    {
        _contrachequeRepository = contrachequeRepository;
    }

    public async Task<IReadOnlyCollection<ContrachequeOutputDTO>> ExecutarAsync(
        Guid fechamentoFolhaId,
        CancellationToken ct = default)
    {
        if (fechamentoFolhaId == Guid.Empty)
            throw new ArgumentException("FechamentoFolhaId não pode ser vazio.", nameof(fechamentoFolhaId));

        var lista = await _contrachequeRepository.ListarPorFechamentoAsync(fechamentoFolhaId, ct);

        return lista
            .Select(GerarContrachequeUseCase.ToOutputDTO)
            .ToList()
            .AsReadOnly();
    }
}
