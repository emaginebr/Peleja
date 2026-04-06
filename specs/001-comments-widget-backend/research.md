# Research: Backend do Widget de Comentários

**Branch**: `001-comments-widget-backend`
**Date**: 2026-04-05

## 1. Paginação por Cursor (Keyset Pagination)

**Decision**: Usar keyset pagination baseada em `comment_id` (bigint) para ordenação por recentes, e `(like_count DESC, comment_id DESC)` para ordenação por populares.

**Rationale**: Keyset pagination é mais performante que offset pagination em grandes volumes de dados, pois não precisa "pular" registros. O cursor é o último `comment_id` retornado (ou par `like_count,comment_id`), garantindo consistência mesmo com inserções concorrentes.

**Alternatives considered**:
- Offset/limit: Simples mas performance degrada com offset alto e inconsistência com dados novos
- Cursor baseado em timestamp: Possibilidade de colisão em timestamps iguais

## 2. Rate Limiting

**Decision**: Usar middleware customizado com `AspNetCoreRateLimit` configurando:
- 10 comentários/minuto por usuário autenticado
- 30 requisições/minuto por IP para endpoints de escrita
- Header `Retry-After` no response 429

**Rationale**: AspNetCoreRateLimit é uma biblioteca madura e amplamente usada no ecossistema .NET. Configuração via appsettings permite ajustes sem deploy.

**Alternatives considered**:
- Rate limiting no API Gateway: Não aplicável (projeto standalone)
- Custom middleware sem biblioteca: Mais trabalho e propenso a bugs

## 3. Integração Giphy

**Decision**: Usar HttpClient direto para consumir a Giphy API v1 (`api.giphy.com/v1/gifs/search`). A chave da API é configurada via variável de ambiente `GIPHY_API_KEY`.

**Rationale**: A Giphy API é simples (1 endpoint de busca) e não justifica uma dependência de SDK. HttpClient via IHttpClientFactory é o padrão .NET para integrações HTTP.

**Alternatives considered**:
- GiphyDotNet SDK: Adiciona dependência desnecessária para 1 endpoint
- Proxy direto no frontend: Exporia a API key

## 4. Multi-Tenant com NAuth

**Decision**: Implementar TenantMiddleware que extrai `X-Tenant-Id` do header e propaga via `TenantContext` (scoped). O NAuth é configurado por tenant com secrets diferentes. O filtro de tenant é aplicado automaticamente nas queries via EF Core query filter global.

**Rationale**: Baseado na skill `dotnet-multi-tenant`. O middleware centraliza a lógica de identificação do tenant. Query filters globais do EF Core garantem isolamento automático sem precisar filtrar manualmente em cada repository.

**Alternatives considered**:
- Database por tenant: Over-engineering para o volume esperado
- Schema por tenant: Complexidade desnecessária de migrations

## 5. Soft Delete

**Decision**: Flag `is_deleted` (boolean, default false) na tabela `comments`. Comentários deletados mantêm o registro mas o conteúdo é substituído por indicação genérica na API response. Query filter global exclui deletados da contagem de likes.

**Rationale**: Soft delete preserva integridade referencial para respostas aninhadas. O campo `deleted_at` (timestamp nullable) complementa para auditoria.

**Alternatives considered**:
- Hard delete com cascade: Perderia respostas
- Hard delete com reparent: Complexo e confuso para o usuário

## 6. Testes de API (Flurl.Http)

**Decision**: Projeto separado `Peleja.API.Tests` usando:
- Flurl.Http para requisições HTTP fluent
- xUnit como test runner
- FluentAssertions para assertions legíveis
- `AuthFixture` (IAsyncLifetime) que faz login uma vez e compartilha o token via `ICollectionFixture`
- Configurações (URL base, credenciais de teste) em `TestSettings.cs` carregadas de `appsettings.Testing.json`

**Rationale**: Flurl.Http oferece API fluent ideal para testes de API. O fixture de autenticação garante que o login ocorre apenas uma vez por sessão. Projeto separado mantém independência.

**Alternatives considered**:
- WebApplicationFactory (in-memory): Não testa a stack HTTP real
- RestSharp: API menos fluent que Flurl

## 7. Emojis e Unicode

**Decision**: PostgreSQL com encoding UTF-8. Colunas de conteúdo como `varchar(2000)`. Sem tratamento especial no backend — emojis são caracteres Unicode padrão que PostgreSQL suporta nativamente.

**Rationale**: PostgreSQL com UTF-8 suporta todo o range Unicode incluindo emojis (UTF-32 surrogate pairs). Não é necessário encoding/decoding especial.

**Alternatives considered**:
- Encoding base64 do conteúdo: Desnecessário e impede busca textual
- Campo separado para emojis: Over-engineering
