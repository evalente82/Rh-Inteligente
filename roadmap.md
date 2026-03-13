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
* **Status Geral:** ✅ Concluído

---

#### 📦 Histórico de Entregas

##### `feat(infra-dev): provisiona container Postgres e aplica migration InitialCreate` — 13/03/2026
> Container Docker `vcorp_rh_dev` (postgres:15-alpine) criado na porta 5435 com restart automático.
> Migration `InitialCreate` aplicada com sucesso: 4 tabelas criadas no banco `vcorp_rh_dev`.
> Connection string do `appsettings.Development.json` atualizada para as novas credenciais.

**Detalhes do container:**
- **Nome:** `vcorp_rh_dev`
- **Imagem:** `postgres:15-alpine`
- **Porta host:** `5435` → `5432` (container)
- **Database:** `vcorp_rh_dev`
- **Usuário:** `vcorp` / **Senha:** `vcorp123`
- **Restart policy:** `unless-stopped`

**Tabelas criadas:**
- `public.Funcionarios` — agregado raiz com TurnoTrabalho (OwnsOne) e EmpresaId (multi-tenant)
- `public.RegistrosPonto` — batidas de ponto com TipoBatida como string
- `public.AlertasAnomalia` — alertas gerados pela IA com gravidade 1–3
- `public.__EFMigrationsHistory` — controle de migrations do EF Core

**Arquivo atualizado:**
- `src/API/appsettings.Development.json` — `Port=5435`, `Username=vcorp`, `Password=vcorp123`

---

##### `feat(mobile): implementa app Flutter — MeusRegistrosPonto com PontoApiService` — 13/03/2026
> App Flutter do Módulo 1 completo: Flutter 3.32.4 / Dart 3.8.1, http + intl + flutter_localizations.
> Tela MeusRegistrosPontoScreen com FutureBuilder, filtro de período (DatePicker PT-BR),
> ListView de anomalias com chips de gravidade (Informativo/Atenção/Crítico) e status (Pendente/Resolvido).
> PontoApiService espelha exatamente o endpoint GET da API C#. flutter analyze: 0 issues.

**Arquivos entregues:**
- `src/mobile/lib/models/alerta_anomalia_model.dart` — espelha `AlertaAnomaliaOutputDTO`; factory `fromJson`
- `src/mobile/lib/services/ponto_api_service.dart` — HTTP GET para `/api/folhaponto/{empresaId}/funcionarios/{funcionarioId}/anomalias`; trata ProblemDetails RFC 7807
- `src/mobile/lib/screens/meus_registros_ponto_screen.dart` — `StatefulWidget`, FutureBuilder, DatePicker PT-BR, `ListView.builder` com `_CardAnomalia`, `_ChipGravidade`, `_ChipStatus`
- `src/mobile/lib/main.dart` — `VcorpApp` com `MaterialApp` tema índigo (Material 3), `flutter_localizations` PT-BR, rota para `MeusRegistrosPontoScreen`

---

##### `feat(frontend): implementa Frontend React — UploadFolhaPonto e DashboardAnomalias` — 13/03/2026
> Frontend Web do Módulo 1 completo: Vite + React 19 + TypeScript + Tailwind CSS v4.
> Página UploadFolhaPonto com Dropzone (drag-and-drop) chamando POST 202 Accepted.
> Página DashboardAnomalias com tabela, badges de gravidade e cards de resumo.
> Build de produção com 0 erros e 19 módulos transformados.

**Arquivos entregues:**
- `src/frontend/vite.config.ts` — proxy `/api` → API .NET, plugin `@tailwindcss/vite`
- `src/frontend/src/index.css` — `@import 'tailwindcss'` (Tailwind v4)
- `src/frontend/src/api/folhaPontoApi.ts` — cliente HTTP tipado: `uploadResumoFolha` e `buscarAnomalias`
- `src/frontend/src/pages/UploadFolhaPonto.tsx` — Dropzone, seletor de período, feedback 202 Accepted
- `src/frontend/src/pages/DashboardAnomalias.tsx` — tabela de alertas, badge de gravidade (1/2/3), cards de resumo
- `src/frontend/src/App.tsx` — navegação entre páginas com estado local

##### `feat(infrastructure+migrations): solution, migration InitialCreate e design-time factory` — 13/03/2026
> Solution file consolidando todos os projetos. Migration InitialCreate gerada pelo EF Core 8
> via IDesignTimeDbContextFactory (stub DesignTimeTenantProvider). 3 tabelas: Funcionarios
> (com TurnoTrabalho como OwnsOne), RegistrosPonto e AlertasAnomalia.

**Arquivos entregues:**
- `Rh-Inteligente.sln` — solution com Domain, Application, Infrastructure, API e Domain.Tests
- `src/Infrastructure/Persistence/RhInteligenteDbContextDesignTimeFactory.cs` — factory design-time com stub ITenantProvider
- `src/Infrastructure/Persistence/Migrations/20260313052736_InitialCreate.cs` — migration completa das 3 tabelas
- `src/Infrastructure/Persistence/Migrations/RhInteligenteDbContextModelSnapshot.cs` — snapshot do modelo EF

---

##### `feat(infrastructure): implementa camada de persistência — EF Core 8, repositórios e multi-tenant` — 13/03/2026
> Camada Infrastructure do Módulo 1 completa: DbContext com Global Query Filters por EmpresaId
> (Regra 5 inegociável), 3 EntityTypeConfigurations mapeadas para Postgres, repositórios concretos,
> EfUnitOfWork, ITenantProvider via HttpContext e InfrastructureServiceExtensions para registro no DI.
> Build e 58/58 testes passando sem regressões.

**Arquivos entregues:**
- `src/Infrastructure/Infrastructure.csproj` — net8.0, EF Core 8.0.0, Npgsql 8.0.0, FrameworkReference AspNetCore.App
- `src/Infrastructure/ITenantProvider.cs` — contrato de resolução do tenant ativo por requisição
- `src/Infrastructure/HttpContextTenantProvider.cs` — resolve EmpresaId via `Request.RouteValues["empresaId"]`
- `src/Infrastructure/InfrastructureServiceExtensions.cs` — `AddInfrastructure()`: registra DbContext, repositórios, UnitOfWork e TenantProvider
- `src/Infrastructure/Persistence/RhInteligenteDbContext.cs` — DbContext com Global Query Filters multi-tenant e `ApplyConfigurationsFromAssembly`
- `src/Infrastructure/Persistence/EfUnitOfWork.cs` — `IUnitOfWork` delegando para `SaveChangesAsync`
- `src/Infrastructure/Persistence/Configurations/FuncionarioConfiguration.cs` — TurnoContratual mapeado como OwnsOne, índices únicos por Matricula/Cpf + EmpresaId
- `src/Infrastructure/Persistence/Configurations/RegistroPontoConfiguration.cs` — TipoBatida como string, índice de performance por FuncionarioId + DataHora
- `src/Infrastructure/Persistence/Configurations/AlertaAnomaliaConfiguration.cs` — TipoAnomalia como string, índices por Resolvido/Gravidade para o dashboard
- `src/Infrastructure/Persistence/Repositories/FuncionarioRepository.cs` — implementa `IFuncionarioRepository` com EF Core 8
- `src/Infrastructure/Persistence/Repositories/RegistroPontoRepository.cs` — implementa `IRegistroPontoRepository` com filtro por período via DateOnly→DateTime
- `src/API/API.csproj` — adicionada ProjectReference para Infrastructure.csproj
- `src/API/Program.cs` — adicionada chamada `builder.Services.AddInfrastructure(builder.Configuration)`
- `src/API/appsettings.json` — connection string `Postgres` adicionada
- `src/API/appsettings.Development.json` — connection string para banco de desenvolvimento

---

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

* **Camada de Dados (Postgres - Infrastructure):** ✅ Concluído
  * `RhInteligenteDbContext`: Global Query Filter por EmpresaId em todas as entidades (Regra 5)
  * Repositórios: `FuncionarioRepository`, `RegistroPontoRepository`
  * `EfUnitOfWork`: `SaveChangesAsync` atômico por Use Case
  * `ITenantProvider` / `HttpContextTenantProvider`: resolução do tenant via rota HTTP
  * Configurações EF: `FuncionarioConfiguration` (OwnsOne TurnoTrabalho), `RegistroPontoConfiguration`, `AlertaAnomaliaConfiguration`

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

* **Camada Frontend (React/Flutter):** ✅ Concluído
  * ✅ React: Página `UploadFolhaPonto` (Dropzone + chamada POST 202 Accepted)
  * ✅ React: Página `DashboardAnomalias` (tabela de anomalias, badges de gravidade, cards de resumo)
  * ✅ Cliente HTTP tipado `folhaPontoApi.ts` (mirrors DTOs da API)
  * ✅ Flutter: Tela `MeusRegistrosPonto` (FutureBuilder, filtro de período, cards com chips de gravidade/status)

* **Gaps Identificados:**
  * `TurnoTrabalho` não suporta turnos que cruzam a meia-noite (ex: 22h–06h). Para turnos noturnos, a `HoraSaida` deve ser representada como `DateTime` do dia seguinte — a ser tratado no Use Case de análise.
  * `CalculoHoraExtraService.CalcularHorasNoturnas()` usa iteração por minuto (O(n)); candidato a otimização antes de entrar em produção.

### Módulo 2: Gestão de Funcionários e Admissão
* **Descrição:** CRUD completo de funcionários, controle de admissão/demissão, regimes de contratação (CLT/PJ/Estágio). Base obrigatória para os perfis Colaborador e Gestor operarem de forma autônoma.
* **Status Geral:** ✅ Concluído

---

#### 📦 Histórico de Entregas

##### `feat(modulo2): implementa Gestão de Funcionários — Domain, Application, Infrastructure e API` — 13/03/2026
> Módulo 2 completo: Value Objects `Cpf` (validação módulo 11) e `Endereco`, entidade `Admissao`,
> enum `RegimeContratacao`, 4 Use Cases com testes, persistência EF Core 8 com owned types,
> migration `AddAdmissaoAndCpfVO` e `FuncionarioController` com 5 endpoints REST.
> 95/95 testes unitários passando. Solução completa sem erros de build.

**Camada Domain (entregues):**
- `src/Domain/Enums/RegimeContratacao.cs` — Clt=1, Pj=2, Estagio=3, Temporario=4
- `src/Domain/ValueObjects/Cpf.cs` — sealed record; validação dígito verificador (módulo 11); rejeita sequências repetidas; `NumeroFormatado` (000.000.000-00); `TentarCriar()` sem throw
- `src/Domain/ValueObjects/Endereco.cs` — sealed record; Logradouro, Numero, Bairro, Cidade, Uf (2 chars), Cep (8 dígitos), Complemento?; `CepFormatado`
- `src/Domain/Entities/Admissao.cs` — factory `Criar(...)`, `Demitir(DateOnly)`, `ReajustarSalario(decimal)`, `AtualizarEndereco(Endereco)`; guard: salário > 0, DataAdmissao máx. 30 dias no futuro
- `src/Domain/Entities/Funcionario.cs` — ATUALIZADO: `Cpf` mudou de `string` para `Cpf` VO; adicionada lista `Admissoes` e propriedade `AdmissaoAtiva`

**Camada Application (entregues):**
- `src/Application/DTOs/CadastrarFuncionarioInputDTO.cs` — EmpresaId, Nome, Cpf (string), Matricula, DataAdmissao, HoraEntrada, HoraSaida, IntervaloAlmocoMinutos
- `src/Application/DTOs/AdmitirFuncionarioInputDTO.cs` — EmpresaId, FuncionarioId, Cargo, SalarioBase, Regime, DataAdmissao + campos de Endereco
- `src/Application/DTOs/FuncionarioOutputDTO.cs` — projeção completa com dados de admissão ativa
- `src/Application/Interfaces/IAdmissaoRepository.cs` — `ObterAdmissaoAtivaAsync`, `AdicionarAsync`, `Atualizar`
- `src/Application/UseCases/CadastrarFuncionarioUseCase.cs` — valida CPF, garante matrícula única, persiste e retorna DTO
- `src/Application/UseCases/AdmitirFuncionarioUseCase.cs` — valida funcionário existente, sem admissão ativa, cria Admissao
- `src/Application/UseCases/ListarFuncionariosUseCase.cs` — `ExecutarAsync` (lista) + `ExecutarPorIdAsync` (detalhe)
- `src/Application/UseCases/DemitirFuncionarioUseCase.cs` — encerra Admissao ativa + chama `Funcionario.Demitir()`

**Camada Infrastructure (entregues):**
- `src/Infrastructure/Persistence/Configurations/FuncionarioConfiguration.cs` — ATUALIZADO: Cpf mapeado como OwnsOne com coluna "Cpf" (varchar 11)
- `src/Infrastructure/Persistence/Configurations/AdmissaoConfiguration.cs` — Admissao com EnderecoResidencial mapeado como OwnsOne (7 colunas End*)
- `src/Infrastructure/Persistence/Repositories/AdmissaoRepository.cs` — implementa `IAdmissaoRepository`
- `src/Infrastructure/Persistence/Repositories/FuncionarioRepository.cs` — ATUALIZADO: `Include(f => f.Admissoes)` em todos os métodos
- `src/Infrastructure/Persistence/RhInteligenteDbContext.cs` — ATUALIZADO: `DbSet<Admissao>` + Global Query Filter multi-tenant
- `src/Infrastructure/InfrastructureServiceExtensions.cs` — ATUALIZADO: `AddScoped<IAdmissaoRepository, AdmissaoRepository>()`
- `src/Infrastructure/Persistence/Migrations/20260313133545_AddAdmissaoAndCpfVO.cs` — tabela `Admissoes`, ajuste Cpf varchar(14→11), índice único EmpresaId+Cpf via SQL raw

**Camada API (entregues):**
- `src/API/Controllers/FuncionarioController.cs` — rota `api/{empresaId:guid}/funcionarios`: POST (201), GET (200), GET `/{id}` (200), POST `/{id}/admissao` (200), DELETE `/{id}` (204)
- `src/API/Program.cs` — ATUALIZADO: 4 novos Use Cases registrados como Scoped

**Testes (entregues):**
- `tests/Domain.Tests/ValueObjects/CpfTests.cs` — 9 testes (válidos, inválidos, formatação, módulo 11)
- `tests/Domain.Tests/Entities/AdmissaoTests.cs` — 9 testes (criação, demissão, reajuste, guards)
- `tests/Domain.Tests/UseCases/CadastrarFuncionarioUseCaseTests.cs` — 4 testes
- `tests/Domain.Tests/UseCases/AdmitirFuncionarioUseCaseTests.cs` — 3 testes
- `tests/Domain.Tests/UseCases/ListarFuncionariosUseCaseTests.cs` — 3 testes
- `tests/Domain.Tests/UseCases/DemitirFuncionarioUseCaseTests.cs` — 4 testes
- **Total: 95/95 testes passando** ✅

---

#### Escopo Técnico

| Camada | Status |
|---|---|
| **Domain** | ✅ `Cpf` VO, `Endereco` VO, `Admissao`, `RegimeContratacao`, `Funcionario` atualizado |
| **Application** | ✅ 4 Use Cases + 3 DTOs + `IAdmissaoRepository` |
| **Infrastructure** | ✅ `AdmissaoConfiguration`, `AdmissaoRepository`, migration `AddAdmissaoAndCpfVO` |
| **API** | ✅ `FuncionarioController` — 5 endpoints |
| **Testes** | ✅ 95/95 passando |

---

### Módulo 3: Motor de IA Gemini + RAG (CCT Reader + Auditoria Real)
* **Descrição:** Implementação real de `IAuditorIaService` via Google Gemini 1.5 Flash. Upload de PDF de Convenção Coletiva → vetorização no Qdrant (já no Docker) → RAG para auditoria autônoma de pontos e Dashboard de Risco Trabalhista. Referência: Protótipo HTML — perfil Dono (Termômetro de Conformidade, Agente CCT, Fechador de Folha).
* **Status Geral:** ⏳ Planejado

#### Escopo Técnico

| Camada | Entregas |
|---|---|
| **Infrastructure** | `GeminiAuditorIaService` (impl. `IAuditorIaService`), `QdrantVectorRepository`, `CctPdfParserService` (PDF → chunks → embeddings Gemini) |
| **Application** | `ProcessarCctUseCase`, `GerarHoleriteNarrativoUseCase` |
| **API** | `CctController` — `POST /{empresaId}/cct/upload` (202), `GET /{empresaId}/regras` |
| **Integração** | Qdrant rodando em `localhost:6333` (já no Docker), `GeminiApiKey` via `appsettings` |