# Implementation Plan: Site Entity with ClientId Account Separation

**Branch**: `002-site-entity-clientid` | **Date**: 2026-04-06 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-site-entity-clientid/spec.md`

## Summary

Add a Site entity with a unique ClientId to separate comment accounts per website. Each Site belongs to a user (from NAuth) and maps to a configurable tenant. The `X-Client-Id` header replaces `X-Tenant-Id` on comment endpoints — the tenant is resolved automatically from the Site's configuration. Pages are associated with Sites (no longer have their own UserId). Site administrators can moderate comments on their sites.

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: Entity Framework Core 9.x, NAuth 0.5.10, AutoMapper 13.x, AspNetCoreRateLimit  
**Storage**: PostgreSQL 17 — shared database (Sites) + per-tenant databases (Pages, Comments, Likes)  
**Testing**: xUnit, Moq, FluentAssertions, Flurl.Http  
**Target Platform**: Linux Docker containers (emagine-network)  
**Project Type**: Web service (REST API)  
**Performance Goals**: Site lookup < 100ms, no degradation on comment queries  
**Constraints**: Multi-tenant with per-tenant DB isolation  
**Scale/Scope**: 2 tenants (emagine, peleja), multiple sites per tenant

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| .NET 8.0 + EF Core | PASS | Using existing stack |
| PostgreSQL conventions (snake_case, bigint PK) | PASS | Site table follows conventions |
| NAuth authentication | PASS | Using ITenantSecretProvider for per-tenant JWT |
| timestamp without time zone | PASS | All timestamp columns |
| ClientSetNull delete behavior | PASS | No cascade deletes |
| AutoMapper for mapping | PASS | Separate profiles per entity |
| File-scoped namespaces | PASS | Project convention |
| Use dotnet-architecture skill | PASS | Will use for Site entity implementation |

No violations. Gate passed.

## Project Structure

### Documentation (this feature)

```text
specs/002-site-entity-clientid/
├── plan.md              # This file
├── spec.md              # Feature specification
├── research.md          # Phase 0 research decisions
├── data-model.md        # Phase 1 data model
├── quickstart.md        # Phase 1 implementation guide
├── contracts/
│   └── site-endpoints.md # API contracts
└── checklists/
    └── requirements.md  # Spec quality checklist
```

### Source Code (repository root)

```text
Peleja.API/
├── Controllers/
│   └── SiteController.cs          # NEW: Site CRUD endpoints
├── Middleware/
│   ├── TenantMiddleware.cs        # MODIFIED: also handles X-Client-Id
│   └── ClientIdMiddleware.cs      # NEW: resolves Site → Tenant from X-Client-Id

Peleja.Application/
├── Services/
│   ├── TenantContext.cs           # MODIFIED: reads SiteId from context
│   ├── TenantProvider.cs          # EXISTING
│   └── TenantSecretProvider.cs    # EXISTING

Peleja.Domain/
├── Enums/
│   └── SiteStatus.cs              # NEW: Active, Blocked, Inactive
├── Models/
│   ├── SiteModel.cs               # NEW: rich domain model
│   ├── PageModel.cs               # MODIFIED: remove UserId, add SiteId
│   └── CommentModel.cs            # EXISTING
├── Mappings/
│   └── SiteResultProfile.cs       # NEW: SiteModel → SiteResult
├── Services/
│   ├── SiteService.cs             # NEW: CRUD + ClientId generation
│   └── CommentService.cs          # MODIFIED: site admin delete permission

Peleja.DTO/
├── SiteInsertInfo.cs              # NEW
├── SiteUpdateInfo.cs              # NEW
└── SiteResult.cs                  # NEW

Peleja.Infra/
├── Context/
│   ├── SharedContext.cs           # NEW: DbContext for Site table
│   ├── Site.cs                    # NEW: persistence entity
│   ├── Page.cs                    # MODIFIED: remove UserId, add SiteId
│   └── PelejaContext.cs           # MODIFIED: update Page mapping
├── Mappings/
│   ├── SiteMapperProfile.cs       # NEW
│   └── PageMapperProfile.cs       # MODIFIED
├── Repositories/
│   ├── SiteRepository.cs          # NEW
│   └── PageRepository.cs          # MODIFIED

Peleja.Infra.Interfaces/
├── Repositories/
│   └── ISiteRepository.cs         # NEW

Peleja.Tests/
├── Domain/Services/
│   ├── SiteServiceTests.cs        # NEW
│   └── CommentServiceTests.cs     # MODIFIED
├── Infra/Repositories/
│   └── SiteRepositoryTests.cs     # NEW

Peleja.Tests.API/
├── Controllers/
│   └── SiteControllerTests.cs     # NEW
```

**Structure Decision**: Follows existing Clean Architecture. New `SharedContext` for the Site table (cross-tenant). Existing `PelejaContext` remains for per-tenant data.

## Complexity Tracking

No constitution violations to justify.
