namespace Application.DTOs;

/// <summary>
/// Dados de entrada para o upload de um arquivo de resumo de folha de ponto.
/// Recebido pelo FolhaPontoController e repassado ao UploadResumoFolhaUseCase.
/// </summary>
public sealed record UploadResumoFolhaInputDTO(
    /// <summary>Identificador do tenant que está realizando o upload.</summary>
    Guid EmpresaId,

    /// <summary>Nome original do arquivo enviado pelo usuário (ex: "ponto_marco_2026.csv").</summary>
    string NomeArquivo,

    /// <summary>Stream com o conteúdo binário do arquivo.</summary>
    Stream ConteudoArquivo,

    /// <summary>Período de referência — início (ex: 01/03/2026).</summary>
    DateOnly PeriodoInicio,

    /// <summary>Período de referência — fim (ex: 31/03/2026).</summary>
    DateOnly PeriodoFim
);
