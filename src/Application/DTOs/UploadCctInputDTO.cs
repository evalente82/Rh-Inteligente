namespace Application.DTOs;

/// <summary>
/// DTO de entrada para upload de Convenção Coletiva em PDF.
/// </summary>
public sealed record UploadCctInputDTO(
    Guid EmpresaId,
    string NomeArquivo,
    byte[] ConteudoPdf);
