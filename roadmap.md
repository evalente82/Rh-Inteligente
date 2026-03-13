git add . ; git commit -m "feat(domain): implementa base da Domain Layer e interfaces da Application Layer" -m "- Entidades: Funcionario, RegistroPonto, AlertaAnomalia (POCOs puros, .NET 8)" -m "- Value Objects: TurnoTrabalho e IntervaloTempo (records imutáveis)" -m "- Domain Service: CalculoHoraExtraService (HE 50%/100%, horas noturnas CLT)" -m "- Application Interfaces: IFuncionarioRepository, IRegistroPontoRepository, IAuditorIaService, IArmazenamentoArquivoService" -m "- Projects: Domain.csproj e Application.csproj (net8.0, zero dependências externas no Domain)" -m "- Tests: 41/41 unitários passando (xUnit + FluentAssertions)" -m "- Roadmap: status do Módulo 1 atualizado para Em Andamento com histórico de entrega" -m "BREAKING: nenhuma — primeira iteração do módulo"# Vcorp Sistem (Módulo RH) - Cérebro, Regras e Roadmap

## 1. Visão Geral do Produto
O Vcorp Folha IA é um SaaS de vanguarda focado na gestão estratégica de Recursos Humanos e Departamento Pessoal para PMEs. Nosso diferencial absoluto é a aplicação de Inteligência Artificial Autônoma (Agentic AI) para auditar, prever riscos e automatizar rotinas complexas (como leitura de convenções coletivas e fechamento de folha). 

O sistema foi concebido para resolver problemas estruturais com alta eficiência e precisão técnica. Nosso foco é entregar uma ferramenta analítica robusta que resolva os problemas de vez, eliminando soluções rasas do mercado, como os chatbots. O sistema atua de forma nativa e invisível (orquestrando cálculos e apontando divergências), entregando uma experiência de interface limpa, direta e sem interações conversacionais desnecessárias.

O desenvolvimento será rigorosamente modular, limpo e escalável, com foco máximo na padronização. O Módulo 1 (Ponto e Folha) é o ponto de partida.

## 2. Pilha Tecnológica Base e Padrões de Projeto
* **Backend:** C# .NET 8/9 (Web API).
* **Frontend Web:** ReactJS com TypeScript (Tailwind CSS).
* **Frontend Mobile:** Flutter (Dart).
* **Bancos de Dados:** PostgreSQL (Relacional Transacional) e Qdrant (Vetorial para IA/RAG).
* **Padrões Arquiteturais:** Clean Architecture, Domain-Driven Design (DDD), SOLID, CQRS (separação de leitura/escrita) e Repository Pattern.
* **Testes:** xUnit e Moq (Backend), Jest e React Testing Library (Frontend), testes de widget (Flutter).

## 3. Dicionário de Domínio e Regras Inegociáveis
* **Regra 1 (Nomenclatura Padrão Mínimo/Máximo):** Nomes de tabelas, entidades, propriedades e métodos/funções devem ser estritamente em **Português** (idioma do negócio). Termos técnicos, sufixos de design patterns e nomenclatura de infraestrutura devem ser em **Inglês**. Exemplo: `FuncionarioRepository`, `AnalisarPontoUseCase`, `FolhaPontoController`.
* **Regra 2 (Clean Architecture):** O núcleo do sistema (`Domain`) não pode ter NENHUMA dependência de frameworks externos (nem Entity Framework, nem bibliotecas de IA). O `Domain` contém apenas as Entidades (ex: `Funcionario`, `RegistroPonto`) e as regras de negócio puras (ex: cálculo de horas noturnas).
* **Regra 3 (Processamento Assíncrono):** O upload de resumos de folhas de ponto (quinzenal/mensal) deve ser tratado em *Background Jobs* (ex: Hangfire, RabbitMQ ou Azure Service Bus). A API deve retornar um `202 Accepted` rapidamente e processar a IA em segundo plano para não travar o Frontend.
* **Regra 4 (IA como Auditora Invisível e Fim dos Chatbots):** A IA atua exclusivamente como um motor de validação. Ela recebe os dados de ponto do banco, cruza com as regras (CLT/CCT), e devolve um array de "Anomalias" (ex: falta de almoço, risco de dobra). Não haverá qualquer opção de chatbot no sistema; a IA preenche relatórios, dashboards de risco e holerites narrativos de forma autônoma.
* **Regra 5 (Isolamento de Dados - Multi-tenant):** Toda e qualquer tabela transacional no PostgreSQL DEVE possuir um `EmpresaId` (Tenant). As consultas via Entity Framework devem ter um Global Query Filter para garantir que dados de empresas diferentes nunca se misturem.
* **Regra 6 (Minimalismo e Organização de Arquivos):** Não crie arquivos, classes ou abstrações desnecessárias. Mantenha a estrutura estritamente enxuta e funcional. Qualquer documentação adicional, diagramas arquiteturais, manuais ou notas de versão devem ser armazenados exclusivamente em uma pasta raiz chamada `/docs`.
* **Regra 7 (Cobertura de Testes Obrigatória):** É inegociável manter uma cobertura de testes superior a 80% no projeto. As camadas `Domain` e `Application` (UseCases e Services) exigem 100% de Unit Tests. Controladores e acessos a banco de dados exigem Integration Tests rigorosos.

## 4. Mapeamento de Funcionalidades (De/Para)

### Módulo 1: Auditoria Inteligente de Ponto e Folha
* **Descrição:** Upload de arquivos de ponto (csv/xlsx/txt), processamento, cruzamento de regras e devolução de alertas/erros gerados pela IA para o Gestor/Dono.
* **Status Geral:** � Em Andamento

---

#### 📦 Histórico de Entregas

##### `feat(api): implementa camada de apresentação — FolhaPontoController, GlobalExceptionHandler e Program.cs` — 13/03/2026
> Camada API (Presentation) do Módulo 1 completa: controller RESTful com dois endpoints
> (POST upload 202 Accepted e GET anomalias 200 OK), tratamento global de exceções nativo
> do ASP.NET Core 8 via IExceptionHandler com ProblemDetails (RFC 7807) e
> configuração mínima de Swagger/OpenAPI. Sem nenhuma lógica de negócio no controller.

**Arquivos entregues:**
- `src/API/API.csproj` — projeto SDK.Web net8.0 com Swashbuckle 6.5.0
- `src/API/Program.cs` — bootstrap mínimo: controllers, Swagger, ExceptionHandler, Use Cases (Scoped)
- `src/API/Middleware/GlobalExceptionHandler.cs` — IExceptionHandler: ArgumentException→400, InvalidOperationException→422, KeyNotFoundException→404, genérico→500
- `src/API/Controllers/FolhaPontoController.cs` — POST `{empresaId}/upload` (202) e GET `{empresaId}/funcionarios/{funcionarioId}/anomalias` (200)

---

##### `feat(application): implementa Use Cases, DTOs e interfaces de infraestrutura da Application Layer` — 13/03/2026
> Camada Application do Módulo 1 completa: dois Use Cases com fluxo assíncrono (Regra 3),
> DTOs de entrada/saída, IUnitOfWork e IAnalisadorBackgroundService.
> Suite de testes unitários com 58/58 passando (xUnit + FluentAssertions + Moq).

**Arquivos entregues:**
- `src/Application/DTOs/UploadResumoFolhaInputDTO.cs` — record de entrada para upload de arquivo
- `src/Application/DTOs/AnalisarRegistrosInputDTO.cs` — record de entrada para análise de IA
- `src/Application/DTOs/AlertaAnomaliaOutputDTO.cs` — projeção de saída sem expor entidade de domínio
- `src/Application/DTOs/OperacaoAceitaOutputDTO.cs` — resposta padrão 202 Accepted com ProcessoId
- `src/Application/Interfaces/IUnitOfWork.cs` — abstração de controle de transação
- `src/Application/Interfaces/IAnalisadorBackgroundService.cs` — contrato para enfileirar job de IA
- `src/Application/UseCases/UploadResumoFolhaUseCase.cs` — aceita arquivo, persiste no storage, enfileira job; retorna 202
- `src/Application/UseCases/AnalisarRegistrosComIaUseCase.cs` — orquestra IA, persiste alertas, commita transação
- `tests/Domain.Tests/UseCases/UploadResumoFolhaUseCaseTests.cs` — 7 testes unitários
- `tests/Domain.Tests/UseCases/AnalisarRegistrosComIaUseCaseTests.cs` — 10 testes unitários

---

##### `feat(domain): implementa base da Domain Layer e interfaces da Application Layer` — 13/03/2026
> Scaffolding inicial completo das camadas Domain e Application do Módulo 1.
> Todos os arquivos são POCOs puros (.NET 8), sem nenhuma dependência externa na camada de domínio.
> Suite de testes unitários com 41/41 passando (xUnit + FluentAssertions).

**Arquivos entregues:**
- `src/Domain/Domain.csproj` — projeto .NET 8, zero dependências externas
- `src/Domain/Enums/TipoBatida.cs` — enum: Entrada, SaidaAlmoco, RetornoAlmoco, Saida
- `src/Domain/Enums/TipoAnomalia.cs` — enum: 6 tipos de anomalia auditáveis pela IA
- `src/Domain/ValueObjects/TurnoTrabalho.cs` — record imutável com cálculo de `CargaHorariaDiaria`
- `src/Domain/ValueObjects/IntervaloTempo.cs` — record imutável com `MinimoLegalClt` e `RespeitaMinimo()`
- `src/Domain/Entities/Funcionario.cs` — agregado raiz com factory method, `EmpresaId`, `Demitir()`, `AtualizarTurno()`
- `src/Domain/Entities/RegistroPonto.cs` — batida bruta com validação de lançamento manual obrigatória
- `src/Domain/Entities/AlertaAnomalia.cs` — resultado da IA com gravidade 1–3 e `MarcarComoResolvido()`
- `src/Domain/Services/CalculoHoraExtraService.cs` — HE 50%/100%, horas noturnas (22h–05h CLT), `ResultadoCalculoHoraExtra`
- `src/Application/Application.csproj` — projeto .NET 8 com referência ao Domain
- `src/Application/Interfaces/IFuncionarioRepository.cs` — contrato de persistência do agregado Funcionario
- `src/Application/Interfaces/IRegistroPontoRepository.cs` — busca por dia e por período (tenant-safe)
- `src/Application/Interfaces/IAuditorIaService.cs` — contrato do motor de IA (impl. na Infrastructure)
- `src/Application/Interfaces/IArmazenamentoArquivoService.cs` — contrato de upload/storage de arquivos
- `tests/Domain.Tests/GlobalUsings.cs` — global usings: xUnit, FluentAssertions, Moq
- `tests/Domain.Tests/Entities/FuncionarioTests.cs` — 10 testes unitários
- `tests/Domain.Tests/Entities/RegistroPontoTests.cs` — 7 testes unitários
- `tests/Domain.Tests/Entities/AlertaAnomaliaTests.cs` — 6 testes unitários
- `tests/Domain.Tests/ValueObjects/TurnoTrabalhoTests.cs` — 4 testes unitários
- `tests/Domain.Tests/ValueObjects/IntervaloTempoTests.cs` — 5 testes unitários
- `tests/Domain.Tests/Services/CalculoHoraExtraServiceTests.cs` — 9 testes unitários

---

* **Camada de Dados (Postgres - Infrastructure):** ⏳ Pendente
  * Tabelas: `Empresas`, `Funcionarios`, `RegistrosPonto`, `AnomaliasPonto`, `ResumosFolha`

* **Camada de Domínio (C# .NET - Domain):** ✅ Concluído
  * Entidades: `Funcionario`, `RegistroPonto`, `AlertaAnomalia`
  * Value Objects: `TurnoTrabalho`, `IntervaloTempo`
  * Domain Services: `CalculoHoraExtraService`

* **Camada de Aplicação e IA (C# .NET - Application):** ✅ Concluído
  * Interfaces: ✅ `IFuncionarioRepository`, `IRegistroPontoRepository`, `IAuditorIaService`, `IArmazenamentoArquivoService`, `IUnitOfWork`, `IAnalisadorBackgroundService`
  * Casos de Uso: ✅ `UploadResumoFolhaUseCase`, `AnalisarRegistrosComIaUseCase`

* **Camada de Apresentação (C# .NET - API):** ✅ Concluído
  * `Program.cs`: bootstrap mínimo, Swagger, GlobalExceptionHandler, Use Cases registrados (Scoped)
  * `GlobalExceptionHandler`: RFC 7807 ProblemDetails, mapeamento por tipo de exceção
  * `FolhaPontoController`: `POST /{empresaId}/upload` → 202 Accepted | `GET /{empresaId}/funcionarios/{id}/anomalias` → 200 OK

* **Camada Frontend (React/Flutter):** ⏳ Pendente
  * React: Página `UploadFolhaPonto` (Dropzone para os arquivos)
  * React: Página `DashboardAnomalias` (Exibição da tabela de divergências gerada pela IA)
  * Flutter: Tela `MeusRegistrosPonto` (Espelho de ponto do colaborador)

* **Gaps Identificados:**
  * `TurnoTrabalho` não suporta turnos que cruzam a meia-noite (ex: 22h–06h). Para turnos noturnos, a `HoraSaida` deve ser representada como `DateTime` do dia seguinte — a ser tratado no Use Case de análise.
  * `CalculoHoraExtraService.CalcularHorasNoturnas()` usa iteração por minuto (O(n)); candidato a otimização antes de entrar em produção.

### Módulo 2: [Próximo Módulo - Ex: Gestão de Benefícios ou Admissão]
* [A ser mapeado no futuro]