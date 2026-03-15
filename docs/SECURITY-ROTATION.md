# Segurança — Rotação e Revogação de API Keys (Google Gemini)

Este documento descreve a ação executada para remover chaves Google expostas no repositório e as instruções para rotacionar/gerenciar chaves no futuro.

## Resumo das ações executadas (13/03/2026)
- Identificadas chaves expostas no repositório e no histórico Git.
- `git-filter-repo` usado para remover as chaves do histórico e `force push` no repositório remoto.
- Arquivo `appsettings.Development.json` removido do tracking e adicionado ao `.gitignore`.
- Chaves Google (Gemini) revogadas via `gcloud services api-keys delete` para os projetos Dev/Prod.
- Novas chaves foram geradas via `gcloud services api-keys create` com restrição apenas para `generativelanguage.googleapis.com`.
- O arquivo `.env` local foi atualizado com as novas chaves (arquivo não versionado).

## Chaves revogadas
- Projeto `rh-inteligente-dev`: chave `AIzaSyCYOnCCc2AJwOZSFz2q_cvx_dSg2COgMD8` (revogada)
- Projeto `rh-inteligente-prod`: chave `AIzaSyCh7mK9h8N5gY-ZTgH0NEIMEiHZM3TjMz8` (revogada)

## Novas chaves criadas (restritas)
- Projeto `rh-inteligente-dev`: nova chave (restrita a `generativelanguage.googleapis.com`) — gravada em `.env` como `DEV_GEMINI_API_KEY`.
- Projeto `rh-inteligente-prod`: nova chave (restrita a `generativelanguage.googleapis.com`) — gravada em `.env` como `PROD_GEMINI_API_KEY`.

> Observação: As chaves estão em `.env` local e NÃO devem ser versionadas. Nunca copie estas chaves para arquivos rastreados pelo git.

## Comandos usados (exemplos)
> Nota: execute apenas se você tiver as permissões necessárias no projeto GCP e o `gcloud` autenticado.

- Listar projetos disponíveis:
```powershell
gcloud projects list
```

- Listar API keys do projeto:
```powershell
gcloud services api-keys list --project=<PROJECT_ID> --format="table(name,displayName,uid)"
```

- Revogar (deletar) chave exposta:
```powershell
gcloud services api-keys delete projects/<PROJECT_NUMBER>/locations/global/keys/<KEY_UID> --project=<PROJECT_ID> --quiet
```

- Criar nova chave restrita à API Gemini (generativelanguage):
```powershell
gcloud services api-keys create --project=<PROJECT_ID> --display-name="Vcorp-RH-Dev" --api-target=service=generativelanguage.googleapis.com --format="json"
```

- Mostrar a `keyString` retornada no resultado JSON e copiar para `.env` local:
```.env
DEV_GEMINI_API_KEY=<keyString_dev>
PROD_GEMINI_API_KEY=<keyString_prod>
```

## Recomendações e boas práticas
1. Sempre mantenha as chaves no arquivo `.env` ou no Secret Manager do provedor/CI — nunca no repositório.
2. Aplique restrições de uso na API key (apenas serviços necessários — `generativelanguage.googleapis.com`) e, quando possível, restrinja por IP ou referrer.
3. Use o Secret Manager (GCP Secret Manager) para ambientes de produção e injete via IAM/Workload Identity em vez de arquivos .env em produção.
4. Configure policies de rotação periódica (ex.: rotacionar chaves a cada 90 dias) e registre eventos de rotação.
5. Configure um pre-commit hook (ou CI check) que bloqueie commits contendo padrões suspeitos (API keys comummente detectáveis) — ex.: regex para `AIzaSy[A-Za-z0-9_-]{35}`.

## Próximos passos (propostos)
- Adicionar um job GitHub Actions que execute `dotnet build` e `dotnet test` em PRs.
- Adicionar uma verificação na pipeline que rejeite commits contendo chaves (pattern detection).
- Mover chaves de produção para GCP Secret Manager e usar injeção de segredo no CI/CD.

---

Se quiser, eu já crio o workflow GitHub Actions básico para rodar `dotnet build`/`dotnet test` e um simples pre-check que procura por padrões de API keys antes do commit (ou na CI). Quer que eu implemente agora?