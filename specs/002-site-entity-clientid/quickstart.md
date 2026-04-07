# Quickstart: Site Entity Implementation

## Prerequisites

- Existing Peleja project with multi-tenant architecture
- Shared PostgreSQL database for Site table
- Per-tenant databases for Pages, Comments, CommentLikes

## Implementation Order

### Step 1: Shared Database Setup

1. Add `ConnectionStrings:SharedContext` to appsettings
2. Create `SharedContext` DbContext with `Site` entity
3. Create `SiteModel` domain model with rich methods
4. Create `SiteStatus` enum (Active=1, Blocked=2, Inactive=3)
5. Run migration for shared DB

### Step 2: Site Repository & Service

1. Create `ISiteRepository<TModel>` interface
2. Implement `SiteRepository` using `SharedContext`
3. Create `SiteService` with CRUD + ClientId generation
4. Create DTOs: `SiteInsertInfo`, `SiteResult`
5. Create AutoMapper profiles

### Step 3: Middleware Update

1. Create `ClientIdMiddleware` that:
   - Reads `X-Client-Id` header
   - Looks up Site in SharedContext
   - Validates site status
   - Sets `TenantId` (from Site.Tenant) and `SiteId` in HttpContext.Items
2. Update `ITenantContext` to read from the middleware-set items
3. Comment endpoints use `X-Client-Id` instead of `X-Tenant-Id`
4. Site admin endpoints still use `X-Tenant-Id`

### Step 4: Page Entity Update

1. Remove `UserId` from `PageModel` and `Page`
2. Add `SiteId` to `PageModel` and `Page`
3. Update `PelejaContext` mappings (unique on `site_id` + `page_url`)
4. Update `PageRepository` and `PageMapperProfile`

### Step 5: Site Controller

1. Create `SiteController` with POST, GET, PUT endpoints
2. Wire up DI registrations

### Step 6: Comment Service Update

1. Update `CommentService` to receive `siteId` and resolve page by site
2. Update delete permission: add site admin check
3. Update all tests

### Step 7: Bruno Collection & Docs

1. Add Site endpoints to Bruno collection
2. Update API documentation

## Verification

```bash
# Unit tests
dotnet test --filter "FullyQualifiedName~Peleja.Tests."

# API tests (requires running API)
dotnet test --filter "FullyQualifiedName~Peleja.Tests.API"
```
