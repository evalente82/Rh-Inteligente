using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Projeção de saída de um AlertaAnomalia para ser consumida pela API e pelo Frontend.
/// Não expõe a entidade de domínio diretamente (separação de camadas).
/// </summary>
public sealed record AlertaAnomaliaOutputDTO(
    Guid Id,
    Guid FuncionarioId,
    TipoAnomalia TipoAnomalia,
    DateOnly DataReferencia,
    string Descricao,
    int Gravidade,
    DateTime GeradoEm,
    bool Resolvido,
    DateTime? ResolvidoEm
);
