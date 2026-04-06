# Implementation Plan: Backend do Widget de Comentários

**Branch**: `001-comments-widget-backend` | **Date**: 2026-04-05 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-comments-widget-backend/spec.md`

## Summary

Backend API para um widget de comentários embeddável em qualquer website. Suporta leitura pública, criação/edição/exclusão autenticada via NAuth, respostas aninhadas (1 nível), likes (toggle), integração Giphy para GIFs, paginação por cursor, ordenação por recentes/populares, e isolamento multi-tenant via header `X-Tenant-Id`. Inclui testes unitários (xUnit), testes de API (Flurl.Http + xUnit + FluentAssertions) e documentação completa dos endpoints.

## Technical Context

**Language/Version**: C# / .NET 8.0
**Primary Dependencies**: Entity Framework Core 9.x, NAuth (autenticação), zTools, Swashbuckle 8.x (Swagger), AspNetCoreRateLimit (rate limiting), Giphy .NET SDK ou HttpClient direto
**Storage**: PostgreSQL (latest) via EF Core
**Testing**: xUnit (unitários), Flurl.Http + xUnit + FluentAssertions (testes de API)
**Target Platform**: Linux/Windows server (web API)
**Project Type**: Web Service (REST API)
**Performance Goals**: <2s leitura de comentários, <1s paginação, 500 usuários concorrentes
**Constraints**: Sem Docker local, EF Core como único ORM, Clean Architecture (skill dotnet-architecture)
**Scale/Scope**: Multi-tenant, ~500 usuários concorrentes, paginação de 15 itens

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Princípio | Status | Notas |
|-----------|--------|-------|
| I. Stack Tecnológica | PASS | .NET 8, EF Core 9.x, PostgreSQL, NAuth, Swashbuckle — todos conforme |
| II. Convenções de Código | PASS | PascalCase, file-scoped namespaces, `[JsonPropertyName]` camelCase em DTOs |
| III. Convenções de BD | PASS | snake_case, bigint identity, ClientSetNull, varchar com MaxLength |
| IV. Autenticação e Segurança | PASS | NAuth Basic Auth, `[Authorize]` em endpoints protegidos, CORS restrito |
| V. Variáveis de Ambiente | PASS | ConnectionStrings + ASPNETCORE_ENVIRONMENT configurados |
| Tratamento de Erros | PASS | try/catch com StatusCode(500) nos controllers |
| Skill dotnet-architecture | PASS | DEVE ser usado para entidades, services, repos, DTOs, DI |

**Result**: Todos os gates passaram. Nenhuma violação detectada.

## Project Structure

### Documentation (this feature)

```text
specs/001-comments-widget-backend/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── comments-api.md
│   ├── likes-api.md
│   ├── giphy-api.md
│   └── tenants-api.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
Peleja.API/
├── Controllers/
│   ├── CommentController.cs
│   ├── CommentLikeController.cs
│   ├── GiphyController.cs
│   └── TenantController.cs
├── Middleware/
│   ├── TenantMiddleware.cs
│   └── RateLimitMiddleware.cs
├── Program.cs
└── appsettings.json

Peleja.Domain/
├── Models/
│   ├── Tenant.cs
│   ├── User.cs
│   ├── Comment.cs
│   └── CommentLike.cs
├── Services/
│   ├── CommentService.cs
│   ├── CommentLikeService.cs
│   └── GiphyService.cs
└── Enums/
    └── UserRole.cs

Peleja.Infra.Interfaces/
├── Repositories/
│   ├── ITenantRepository.cs
│   ├── IUserRepository.cs
│   ├── ICommentRepository.cs
│   └── ICommentLikeRepository.cs
└── AppServices/
    └── IGiphyAppService.cs

Peleja.Infra/
├── Context/
│   └── PelejaContext.cs
├── Repositories/
│   ├── TenantRepository.cs
│   ├── UserRepository.cs
│   ├── CommentRepository.cs
│   └── CommentLikeRepository.cs
└── AppServices/
    └── GiphyAppService.cs

Peleja.Application/
└── DependencyInjection.cs

Peleja.Domain.Tests/
├── Services/
│   ├── CommentServiceTests.cs
│   ├── CommentLikeServiceTests.cs
│   └── GiphyServiceTests.cs
└── Peleja.Domain.Tests.csproj

Peleja.API.Tests/
├── Controllers/
│   ├── CommentControllerTests.cs
│   ├── CommentLikeControllerTests.cs
│   ├── GiphyControllerTests.cs
│   └── TenantControllerTests.cs
├── Config/
│   ├── TestSettings.cs
│   └── AuthFixture.cs
└── Peleja.API.Tests.csproj
```

**Structure Decision**: Clean Architecture conforme skill `dotnet-architecture` com 5 camadas (API, Domain, Infra.Interfaces, Infra, Application). Testes unitários no projeto `Peleja.Domain.Tests` e testes de API no projeto `Peleja.API.Tests` (Flurl.Http + xUnit + FluentAssertions) com configurações separadas.

## Complexity Tracking

> Nenhuma violação de constituição. Sem justificativas necessárias.
