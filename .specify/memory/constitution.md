<!--
  Sync Impact Report
  ==================
  Version change: 0.0.0 (template) → 1.0.0
  Modified principles: N/A (initial constitution)
  Added sections:
    - Skills Obrigatórias
    - I. Stack Tecnológica
    - II. Convenções de Código
    - III. Convenções de Banco de Dados (PostgreSQL)
    - IV. Autenticação e Segurança
    - V. Variáveis de Ambiente
    - Padrões de Tratamento de Erros
    - Checklist para Novos Contribuidores
    - Governance
  Removed sections: N/A
  Templates requiring updates:
    - .specify/templates/plan-template.md ✅ No changes needed
      (Constitution Check section is dynamic, filled at plan time)
    - .specify/templates/spec-template.md ✅ No changes needed
      (Generic template, no constitution-specific references)
    - .specify/templates/tasks-template.md ✅ No changes needed
      (Generic template, no constitution-specific references)
    - .specify/templates/commands/*.md ✅ No command files exist
  Follow-up TODOs: None
-->

# Peleja Constitution

> Padrões obrigatórios de stack tecnológica, convenções de código e
> arquitetura backend que DEVEM ser seguidos por todos os contribuidores
> em todos os projetos.

## Skills Obrigatórias

Para implementação de novas entidades e funcionalidades, as seguintes
skills **DEVEM** ser utilizadas:

| Skill | Quando usar | Invocação |
|---|---|---|
| **dotnet-architecture** | Criar/modificar entidades, services, repositories, DTOs, migrations, DI no backend | `/dotnet-architecture` |

Estas skills cobrem em detalhe:

- Estrutura de projetos e fluxo de dependência (Clean Architecture backend)
- Regras de repositórios genéricos, mapeamento manual, DI centralizado
- Configuração de DbContext, Fluent API e migrações via `dotnet ef`
- Convenções de nomeação de DTOs (`Info`, `InsertInfo`, `Result`) e
  chaves portuguesas (`sucesso`, `mensagem`, `erros`)

**NÃO** reimplemente esses padrões manualmente — siga as skills.

## Core Principles

### I. Stack Tecnológica

| Tecnologia | Versão | Finalidade |
|---|---|---|
| .NET | 8.0 | Runtime e framework principal |
| Entity Framework Core | 9.x | ORM e migrações |
| PostgreSQL | Latest | Banco de dados relacional |
| NAuth | Latest | Autenticação (Basic token) |
| zTools | Latest | Upload S3, e-mail (MailerSend), slugs |
| Swashbuckle | 8.x | Swagger / OpenAPI |

**Regras de Stack**:

- **NÃO** introduzir ORMs alternativos (Dapper, etc.) — EF Core é o
  único ORM permitido.
- **NÃO** executar comandos `docker` ou `docker compose` no ambiente
  local — Docker não está acessível.

### II. Convenções de Código

#### .NET

| Elemento | Convenção | Exemplo |
|---|---|---|
| Namespaces | PascalCase | `Viralt.Domain.Services` |
| Classes / Interfaces | PascalCase | `CampaignService`, `ICampaignRepository` |
| Métodos | PascalCase | `GetById()`, `MapToDto()` |
| Propriedades | PascalCase | `CampaignId`, `CreatedAt` |
| Campos privados | _camelCase | `_repository`, `_context` |
| Constantes | UPPER_CASE | `BUCKET_NAME` |
| Namespaces | File-scoped | `namespace Viralt.API;` |

#### JSON Property Names

- `[JsonPropertyName("camelCase")]` DEVE ser aplicado em todas as
  propriedades de DTOs.

### III. Convenções de Banco de Dados (PostgreSQL)

| Elemento | Convenção | Exemplo |
|---|---|---|
| Tabelas | snake_case plural | `campaigns`, `campaign_entries` |
| Colunas | snake_case | `campaign_id`, `created_at` |
| Primary Keys | `{entidade}_id`, bigint identity | `campaign_id bigint PK` |
| Constraint PK | `{tabela}_pkey` | `campaigns_pkey` |
| Foreign Keys | `fk_{pai}_{filho}` | `fk_campaign_entry` |
| Delete behavior | `ClientSetNull` | Nunca Cascade |
| Timestamps | `timestamp without time zone` | Sem timezone |
| Strings | `varchar` com MaxLength | `varchar(260)` |
| Booleans | `boolean` com default | `DEFAULT true` |
| Status/Enums | `integer` | `DEFAULT 1` |

Configuração de DbContext, Fluent API e comandos de migração são
detalhados na skill `dotnet-architecture`.

### IV. Autenticação e Segurança

| Aspecto | Padrão |
|---|---|
| Esquema | Basic Authentication via NAuth |
| Header | `Authorization: Basic {token}` |
| Handler | `NAuthHandler` registrado no DI |
| Proteção de rotas | Atributo `[Authorize]` nos controllers |

**Regras de Segurança**:

- **NUNCA** expor connection strings ou secrets em respostas da API.
- Controllers com dados sensíveis DEVEM ter `[Authorize]`.
- CORS configurado como `AllowAnyOrigin` apenas em Development.

### V. Variáveis de Ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `ConnectionStrings__ViraltContext` | Sim | Connection string PostgreSQL |
| `ASPNETCORE_ENVIRONMENT` | Sim | Development, Docker, Production |

## Padrões de Tratamento de Erros

Todos os controllers DEVEM seguir este padrão:

```csharp
try { /* lógica */ }
catch (Exception ex) { return StatusCode(500, ex.Message); }
```

## Checklist para Novos Contribuidores

Antes de submeter qualquer código, verifique:

- [ ] Utilizou a skill `dotnet-architecture` para novas entidades backend
- [ ] Tabelas e colunas seguem snake_case no PostgreSQL
- [ ] Controllers com dados sensíveis possuem `[Authorize]`

## Governance

- Esta constituição tem precedência sobre todas as outras práticas do
  projeto. Em caso de conflito, a constituição prevalece.
- Emendas DEVEM ser documentadas com justificativa, aprovadas pelo
  responsável do projeto e acompanhadas de plano de migração quando
  houver breaking changes.
- Todo PR DEVE verificar conformidade com os princípios listados acima.
- Complexidade além do necessário DEVE ser justificada explicitamente.
- Versionamento segue SemVer: MAJOR para remoções/redefinições de
  princípios, MINOR para adições, PATCH para clarificações.

**Version**: 1.0.0 | **Ratified**: 2026-04-05 | **Last Amended**: 2026-04-05
