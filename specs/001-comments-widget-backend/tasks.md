# Tasks: Backend do Widget de Comentários

**Input**: Design documents from `/specs/001-comments-widget-backend/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/

**Tests**: Testes unitários (xUnit) e testes de API (Flurl.Http + xUnit + FluentAssertions) são OBRIGATÓRIOS conforme spec.

**Organization**: Tasks agrupadas por user story para implementação e teste independentes.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Pode rodar em paralelo (arquivos diferentes, sem dependências)
- **[Story]**: User story à qual a task pertence (US1, US2, etc.)
- Inclui caminhos exatos de arquivos nas descrições

---

## Phase 1: Setup

**Purpose**: Criação da solution e projetos conforme Clean Architecture

- [x] T001 Criar solution `Peleja.sln` e os projetos: `Peleja.API`, `Peleja.Domain`, `Peleja.Infra.Interfaces`, `Peleja.Infra`, `Peleja.Application`, `Peleja.Domain.Tests`, `Peleja.API.Tests` com referências entre projetos conforme skill `dotnet-architecture`
- [x] T002 [P] Instalar pacotes NuGet no `Peleja.API`: Swashbuckle.AspNetCore, AspNetCoreRateLimit, NAuth
- [x] T003 [P] Instalar pacotes NuGet no `Peleja.Infra`: Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design
- [x] T004 [P] Instalar pacotes NuGet no `Peleja.Domain.Tests`: xUnit, xUnit.runner.visualstudio, Moq, FluentAssertions
- [x] T005 [P] Instalar pacotes NuGet no `Peleja.API.Tests`: xUnit, xUnit.runner.visualstudio, Flurl.Http, FluentAssertions
- [x] T006 [P] Configurar `Peleja.API/appsettings.json` e `appsettings.Development.json` com ConnectionStrings (PelejaContext), GIPHY_API_KEY, rate limiting settings e NAuth settings
- [x] T007 [P] Criar `Peleja.API.Tests/Config/TestSettings.cs` com configurações de URL base e credenciais de teste, carregadas de `appsettings.Testing.json`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Infraestrutura core que DEVE estar completa antes de qualquer user story

**CRITICAL**: Nenhuma user story pode iniciar antes desta fase estar completa

- [x] T008 Criar enum `UserRole` (User=1, Moderator=2) em `Peleja.Domain/Enums/UserRole.cs`
- [x] T009 [P] Criar model `Tenant` em `Peleja.Domain/Models/Tenant.cs` conforme data-model.md (tenant_id, name, slug, nauth_api_url, nauth_api_key, is_active, created_at, updated_at)
- [x] T010 [P] Criar model `User` em `Peleja.Domain/Models/User.cs` conforme data-model.md (user_id, tenant_id, nauth_user_id, display_name, avatar_url, role, created_at, updated_at)
- [x] T011 [P] Criar model `Comment` em `Peleja.Domain/Models/Comment.cs` conforme data-model.md (comment_id, tenant_id, user_id, parent_comment_id, page_url, content, gif_url, like_count, is_edited, is_deleted, created_at, updated_at, deleted_at)
- [x] T012 [P] Criar model `CommentLike` em `Peleja.Domain/Models/CommentLike.cs` conforme data-model.md (comment_like_id, comment_id, user_id, created_at)
- [x] T013 Criar `PelejaContext` em `Peleja.Infra/Context/PelejaContext.cs` com DbSets, Fluent API (snake_case, bigint identity, FKs ClientSetNull, indexes, query filters globais para tenant e soft delete) conforme data-model.md e skill `dotnet-architecture`
- [x] T014 [P] Criar interfaces de repositório em `Peleja.Infra.Interfaces/Repositories/`: `ITenantRepository.cs`, `IUserRepository.cs`, `ICommentRepository.cs`, `ICommentLikeRepository.cs` com métodos CRUD e queries específicas
- [x] T015 [P] Criar interface `IGiphyAppService.cs` em `Peleja.Infra.Interfaces/AppServices/` com método SearchAsync(query, limit, offset)
- [x] T016 Implementar repositórios em `Peleja.Infra/Repositories/`: `TenantRepository.cs`, `UserRepository.cs`, `CommentRepository.cs` (com paginação cursor, ordenação recentes/populares, include de replies), `CommentLikeRepository.cs` (com toggle e contagem)
- [x] T017 Criar `TenantContext` (scoped) e `TenantMiddleware` em `Peleja.API/Middleware/TenantMiddleware.cs` — extrai `X-Tenant-Id` do header, valida tenant ativo, propaga tenant_id via TenantContext conforme skill `dotnet-multi-tenant`
- [x] T018 Configurar NAuth com multi-tenant em `Peleja.API/Program.cs` — NAuthHandler usa configurações do tenant corrente (nauth_api_url, nauth_api_key do TenantContext) conforme skills `nauth-guide` e `dotnet-multi-tenant`
- [x] T019 Configurar rate limiting em `Peleja.API/Program.cs` com AspNetCoreRateLimit — 10 comentários/minuto por usuário, 30 req/minuto por IP em endpoints de escrita
- [x] T020 Criar `DependencyInjection.cs` em `Peleja.Application/` registrando todos os repositories, services, appservices, TenantContext, middleware e NAuth no container DI conforme skill `dotnet-architecture`
- [x] T021 Configurar `Program.cs` em `Peleja.API/` com middleware pipeline: TenantMiddleware → NAuth → RateLimit → Controllers → Swagger, CORS (AllowAnyOrigin apenas em Development)
- [x] T022 Criar DTOs compartilhados: `CommentResult`, `CommentInfo`, `CommentInsertInfo`, `CommentUpdateInfo`, `AuthorInfo`, `PaginatedResult<T>` (com cursor), `GiphySearchResult`, `GiphyItemInfo` em `Peleja.Domain/Models/` com `[JsonPropertyName("camelCase")]` e chaves portuguesas (sucesso, mensagem, erros) conforme constituição
- [x] T023 Gerar migration inicial com `dotnet ef migrations add InitialCreate --project Peleja.Infra --startup-project Peleja.API`
- [x] T024 Criar `AuthFixture` em `Peleja.API.Tests/Config/AuthFixture.cs` — IAsyncLifetime que faz login via NAuth uma vez e compartilha token via ICollectionFixture

**Checkpoint**: Infraestrutura pronta — implementação de user stories pode iniciar

---

## Phase 3: User Story 1 - Leitura de Comentários (Priority: P1) MVP

**Goal**: Visitantes não autenticados visualizam comentários paginados por cursor, com ordenação por recentes ou populares

**Independent Test**: GET /api/v1/comments sem token retorna lista paginada com respostas aninhadas

### Unit Tests for User Story 1

- [x] T025 [P] [US1] Criar testes unitários para `CommentService.GetByPageUrlAsync` em `Peleja.Domain.Tests/Services/CommentServiceTests.cs` — testar paginação cursor, ordenação recentes/populares, inclusão de replies, filtro por page_url, soft delete excluído

### API Tests for User Story 1

- [x] T026 [P] [US1] Criar testes de API para GET `/api/v1/comments` em `Peleja.API.Tests/Controllers/CommentControllerTests.cs` — testar listagem pública sem auth, paginação cursor (pageSize=15, nextCursor, hasMore), ordenação por recentes e populares, filtro por pageUrl, respostas aninhadas, comentários deletados exibem "[Comentário removido]"

### Implementation for User Story 1

- [x] T027 [US1] Implementar `CommentService` em `Peleja.Domain/Services/CommentService.cs` — método `GetByPageUrlAsync(pageUrl, sortBy, cursor, pageSize)` com paginação cursor (keyset por comment_id ou like_count,comment_id), inclusão de replies (1 nível), mapeamento para CommentResult com AuthorInfo
- [x] T028 [US1] Implementar `CommentController` (GET) em `Peleja.API/Controllers/CommentController.cs` — endpoint GET `/api/v1/comments` com query params (pageUrl, sortBy, cursor, pageSize), retorna PaginatedResult<CommentResult> com `[AllowAnonymous]`, inclui isLikedByUser se autenticado

**Checkpoint**: US1 funcional — leitura pública de comentários com paginação e ordenação

---

## Phase 4: User Story 2 - Criação de Comentários (Priority: P2)

**Goal**: Usuários autenticados criam comentários com texto, emojis e GIF opcional

**Independent Test**: POST /api/v1/comments com token retorna 201 com comentário criado

### Unit Tests for User Story 2

- [x] T029 [P] [US2] Criar testes unitários para `CommentService.CreateAsync` em `Peleja.Domain.Tests/Services/CommentServiceTests.cs` — testar criação com texto simples, com emojis Unicode, com gifUrl, validação de content max 2000 chars, validação de pageUrl obrigatória

### API Tests for User Story 2

- [x] T030 [P] [US2] Criar testes de API para POST `/api/v1/comments` em `Peleja.API.Tests/Controllers/CommentControllerTests.cs` — testar criação autenticada (201), rejeição sem auth (401), validação de campos (400), rate limit (429), conteúdo com emojis persiste corretamente

### Implementation for User Story 2

- [x] T031 [US2] Implementar `CommentService.CreateAsync` em `Peleja.Domain/Services/CommentService.cs` — validação de content (1-2000 chars), pageUrl obrigatória, gifUrl opcional (max 500), criação vinculada ao tenant e user do contexto
- [x] T032 [US2] Implementar `CommentController` (POST) em `Peleja.API/Controllers/CommentController.cs` — endpoint POST `/api/v1/comments` com `[Authorize]`, recebe CommentInsertInfo, retorna 201 com CommentResult

**Checkpoint**: US1 + US2 funcionais — leitura pública e criação autenticada

---

## Phase 5: User Story 3 - Respostas a Comentários (Priority: P3)

**Goal**: Usuários autenticados respondem a comentários existentes, criando threads de 1 nível

**Independent Test**: POST /api/v1/comments com parentCommentId cria resposta vinculada ao pai

### Unit Tests for User Story 3

- [x] T033 [P] [US3] Criar testes unitários para lógica de respostas em `Peleja.Domain.Tests/Services/CommentServiceTests.cs` — testar criação com parentCommentId válido, rejeição de parentCommentId inexistente, rejeição de resposta a resposta (apenas 1 nível), inclusão de replies na listagem do pai

### API Tests for User Story 3

- [x] T034 [P] [US3] Criar testes de API para respostas em `Peleja.API.Tests/Controllers/CommentControllerTests.cs` — testar POST com parentCommentId (201), parentCommentId inexistente (404), resposta a resposta rejeitada (400), replies aninhadas no GET do pai

### Implementation for User Story 3

- [x] T035 [US3] Estender `CommentService.CreateAsync` em `Peleja.Domain/Services/CommentService.cs` — validar parentCommentId: deve existir, deve ser root (parent_comment_id IS NULL), deve pertencer ao mesmo tenant e page_url
- [x] T036 [US3] Atualizar `CommentController` (POST) em `Peleja.API/Controllers/CommentController.cs` para aceitar parentCommentId no CommentInsertInfo

**Checkpoint**: US1 + US2 + US3 funcionais — leitura, criação e respostas

---

## Phase 6: User Story 4 - Likes em Comentários (Priority: P4)

**Goal**: Usuários autenticados dão/removem like (toggle) em comentários; contagem influencia ordenação por populares

**Independent Test**: POST /api/v1/comments/{id}/like alterna like e atualiza contagem

### Unit Tests for User Story 4

- [x] T037 [P] [US4] Criar testes unitários para `CommentLikeService` em `Peleja.Domain.Tests/Services/CommentLikeServiceTests.cs` — testar toggle like (adicionar), toggle unlike (remover), contagem incrementa/decrementa, unicidade por user+comment, rejeição para comentário inexistente

### API Tests for User Story 4

- [x] T038 [P] [US4] Criar testes de API para POST `/api/v1/comments/{id}/like` em `Peleja.API.Tests/Controllers/CommentLikeControllerTests.cs` — testar like toggle (200), sem auth (401), comentário inexistente (404), isLikedByUser refletido no GET de comentários

### Implementation for User Story 4

- [x] T039 [US4] Implementar `CommentLikeService` em `Peleja.Domain/Services/CommentLikeService.cs` — toggle like/unlike, atualizar like_count no Comment (desnormalizado), verificar unicidade user+comment
- [x] T040 [US4] Implementar `CommentLikeController` em `Peleja.API/Controllers/CommentLikeController.cs` — endpoint POST `/api/v1/comments/{commentId}/like` com `[Authorize]`, retorna likeCount e isLikedByUser
- [x] T041 [US4] Atualizar `CommentService.GetByPageUrlAsync` em `Peleja.Domain/Services/CommentService.cs` para incluir `isLikedByUser` quando o usuário está autenticado (verificar existência de CommentLike para o user corrente)

**Checkpoint**: US1-US4 funcionais — leitura, criação, respostas e likes

---

## Phase 7: User Story 5 - Integração Giphy (Priority: P5)

**Goal**: Endpoint de busca de GIFs no Giphy para enriquecer comentários

**Independent Test**: GET /api/v1/giphy/search?q=feliz retorna lista de GIFs com URLs e previews

### Unit Tests for User Story 5

- [x] T042 [P] [US5] Criar testes unitários para `GiphyService` em `Peleja.Domain.Tests/Services/GiphyServiceTests.cs` — testar busca com resultados, busca sem resultados, tratamento de erro quando Giphy indisponível (503)

### API Tests for User Story 5

- [x] T043 [P] [US5] Criar testes de API para GET `/api/v1/giphy/search` em `Peleja.API.Tests/Controllers/GiphyControllerTests.cs` — testar busca autenticada (200), sem auth (401), parâmetro q ausente (400), paginação (limit, offset)

### Implementation for User Story 5

- [x] T044 [US5] Implementar `GiphyAppService` em `Peleja.Infra/AppServices/GiphyAppService.cs` — HttpClient via IHttpClientFactory para `api.giphy.com/v1/gifs/search`, API key via configuração, mapeamento de resposta para GiphyItemInfo (id, title, url, previewUrl, width, height), tratamento de falha retorna exceção capturada no controller como 503
- [x] T045 [US5] Implementar `GiphyService` em `Peleja.Domain/Services/GiphyService.cs` — orquestra chamada ao IGiphyAppService, mapeamento para GiphySearchResult com paginação (totalCount, offset, limit)
- [x] T046 [US5] Implementar `GiphyController` em `Peleja.API/Controllers/GiphyController.cs` — endpoint GET `/api/v1/giphy/search` com `[Authorize]`, query params (q, limit, offset), retorna 503 quando Giphy indisponível

**Checkpoint**: US1-US5 funcionais — todas features de conteúdo completas

---

## Phase 8: User Story 7 - Edição e Exclusão de Comentários (Priority: P7)

**Goal**: Autor edita/exclui seus comentários; moderador exclui qualquer comentário do tenant

**Independent Test**: PUT e DELETE /api/v1/comments/{id} com verificação de permissões (autor vs moderador)

### Unit Tests for User Story 7

- [x] T047 [P] [US7] Criar testes unitários para `CommentService.UpdateAsync` e `CommentService.DeleteAsync` em `Peleja.Domain.Tests/Services/CommentServiceTests.cs` — testar edição pelo autor (is_edited=true), rejeição de edição por não-autor, soft delete pelo autor, soft delete pelo moderador, rejeição de delete por não-autor/não-moderador, comentário deletado com respostas mantidas

### API Tests for User Story 7

- [x] T048 [P] [US7] Criar testes de API para PUT e DELETE `/api/v1/comments/{id}` em `Peleja.API.Tests/Controllers/CommentControllerTests.cs` — testar edição pelo autor (200), edição por outro usuário (403), exclusão pelo autor (200), exclusão por moderador (200), exclusão por não-autorizado (403), comentário inexistente (404), comentário deletado exibe "[Comentário removido]" no GET

### Implementation for User Story 7

- [x] T049 [US7] Implementar `CommentService.UpdateAsync` em `Peleja.Domain/Services/CommentService.cs` — validação de ownership (user_id == autor), atualizar content e/ou gifUrl, marcar is_edited=true, updated_at
- [x] T050 [US7] Implementar `CommentService.DeleteAsync` em `Peleja.Domain/Services/CommentService.cs` — soft delete: verificar se é autor OU moderador do tenant, marcar is_deleted=true, deleted_at, manter respostas
- [x] T051 [US7] Implementar `CommentController` (PUT, DELETE) em `Peleja.API/Controllers/CommentController.cs` — endpoints PUT e DELETE `/api/v1/comments/{commentId}` com `[Authorize]`, verificação de permissões, retorno adequado (200, 403, 404)

**Checkpoint**: Todas as user stories funcionais

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Documentação da API, testes finais e refinamentos

- [x] T052 [P] Gerar documentação do `CommentController` usando skill `dotnet-doc-controller` em `docs/api/comment-controller.md`
- [x] T053 [P] Gerar documentação do `CommentLikeController` usando skill `dotnet-doc-controller` em `docs/api/comment-like-controller.md`
- [x] T054 [P] Gerar documentação do `GiphyController` usando skill `dotnet-doc-controller` em `docs/api/giphy-controller.md`
- [x] T055 [P] Gerar documentação do `TenantMiddleware` e fluxo de autenticação em `docs/api/authentication.md`
- [x] T056 Criar documento index da API usando skill `doc-manager` em `docs/api/README.md` — consolidar todos os endpoints, exemplos de request/response, fluxo de autenticação e multi-tenant
- [x] T057 Configurar Swagger/OpenAPI com descrições completas dos endpoints, schemas de request/response e exemplos em `Peleja.API/Program.cs`
- [x] T058 Validar quickstart.md executando os comandos de teste manual listados em `specs/001-comments-widget-backend/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: Sem dependências — pode iniciar imediatamente
- **Foundational (Phase 2)**: Depende de Setup — BLOQUEIA todas as user stories
- **US1 (Phase 3)**: Depende de Foundational
- **US2 (Phase 4)**: Depende de Foundational (pode rodar em paralelo com US1 se não houver conflito de arquivos, mas sequencial é recomendado pois US2 estende CommentController)
- **US3 (Phase 5)**: Depende de US2 (estende CreateAsync)
- **US4 (Phase 6)**: Depende de Foundational (pode rodar em paralelo com US1-US3)
- **US5 (Phase 7)**: Depende de Foundational (independente das outras stories)
- **US7 (Phase 8)**: Depende de US2 (estende CommentController e CommentService)
- **Polish (Phase 9)**: Depende de todas as user stories completas

### User Story Dependencies

- **US1 (P1)**: Depende de Foundational — sem dependência de outras stories
- **US2 (P2)**: Depende de Foundational — sem dependência de outras stories
- **US3 (P3)**: Depende de US2 (estende lógica de criação com parentCommentId)
- **US4 (P4)**: Depende de Foundational — sem dependência de outras stories
- **US5 (P5)**: Depende de Foundational — totalmente independente
- **US7 (P7)**: Depende de US2 (estende CommentService e CommentController)

### Within Each User Story

- Testes unitários e testes de API marcados [P] podem rodar em paralelo com a implementação (TDD: escrever primeiro, falhar, depois implementar)
- Models antes de services
- Services antes de controllers
- Core antes de integração

### Parallel Opportunities

- T002, T003, T004, T005, T006, T007 (todos os pacotes e config do Setup)
- T009, T010, T011, T012 (todos os models)
- T014, T015 (todas as interfaces)
- Testes unitários e testes de API de cada story são parallelizáveis entre si
- US4 e US5 podem rodar em paralelo com US1-US3 (arquivos diferentes)
- T052, T053, T054, T055 (toda a documentação)

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete Phase 3: US1 — Leitura de Comentários
4. **STOP and VALIDATE**: Testar leitura pública com paginação e ordenação
5. Deploy/demo se pronto

### Incremental Delivery

1. Setup + Foundational → Infraestrutura pronta
2. US1 (Leitura) → Testar → Deploy (MVP!)
3. US2 (Criação) → Testar → Deploy
4. US3 (Respostas) → Testar → Deploy
5. US4 (Likes) → Testar → Deploy
6. US5 (Giphy) → Testar → Deploy
7. US7 (Edição/Exclusão) → Testar → Deploy
8. Polish (Documentação) → Finalizar

### Parallel Team Strategy

Com múltiplos desenvolvedores após Foundational:

- Dev A: US1 (Leitura) → US2 (Criação) → US3 (Respostas) → US7 (Edição)
- Dev B: US4 (Likes) → US5 (Giphy)
- Ambos: Polish (Documentação)

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências
- [Story] label mapeia task à user story para rastreabilidade
- Cada user story DEVE ser independentemente testável
- Usar skill `dotnet-architecture` para TODA criação de entidades, services, repos, DTOs
- Usar skill `nauth-guide` para configuração de autenticação
- Usar skill `dotnet-multi-tenant` para TenantMiddleware e multi-tenant NAuth
- Usar skill `dotnet-test` para estrutura dos testes unitários
- Usar skills `dotnet-doc-controller` e `doc-manager` para documentação
- Commit após cada task ou grupo lógico
- Parar em cada checkpoint para validar story independentemente
