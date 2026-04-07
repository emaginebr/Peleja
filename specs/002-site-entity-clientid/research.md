# Research: Site Entity with ClientId

## R-001: Site Storage Location (Shared vs Per-Tenant Database)

**Decision**: Site entity stored in a **shared/central database**, not per-tenant.

**Rationale**: The Site entity contains the tenant mapping (`Tenant` field). It must be accessible before the tenant is resolved — the `X-Client-Id` header triggers a Site lookup to determine which tenant database to use. Storing it in a per-tenant DB would create a chicken-and-egg problem.

**Alternatives considered**:
- Store in every tenant DB (rejected: duplication, sync issues, can't resolve tenant without knowing which DB to check)
- Store in configuration/file (rejected: not dynamic, can't be managed via API)

**Implementation**: A separate `SharedContext` DbContext with its own connection string (`ConnectionStrings:SharedContext`) for the Site table only.

## R-002: ClientId Format

**Decision**: Use `Guid.NewGuid().ToString("N")` (32-char hex string without dashes).

**Rationale**: GUIDs are globally unique, don't require coordination, and the "N" format is URL-safe and compact. No sequential guessing risk.

**Alternatives considered**:
- UUID with dashes (rejected: longer, less clean in headers)
- Short random string (rejected: collision risk at scale)
- Auto-increment (rejected: guessable, security concern)

## R-003: Tenant Resolution Flow Change

**Decision**: Replace `X-Tenant-Id` header with `X-Client-Id` for comment endpoints. The tenant is resolved from `Site.Tenant`.

**Rationale**: The user specified that ClientId is the way to separate accounts. The tenant becomes an internal implementation detail resolved from the site configuration.

**Flow**:
1. Request arrives with `X-Client-Id` header
2. Middleware looks up Site in shared DB by ClientId
3. Validates site status (Active/Inactive/Blocked)
4. Resolves tenant from `Site.Tenant`
5. Sets `TenantId` and `SiteId` in `HttpContext.Items`
6. Downstream services use `ITenantContext` to get tenant and connect to correct DB

**Site admin endpoints** (`/api/v1/sites`) use JWT `tenant_id` claim for auth but don't need `X-Client-Id`.

## R-004: Site Admin Moderation

**Decision**: Comment delete permission check: author OR NAuth IsAdmin OR site administrator (Site.UserId matches current user).

**Rationale**: The site owner is responsible for content on their site. They need moderation power without requiring NAuth admin status.

**Implementation**: `CommentService.DeleteAsync` receives the site's UserId and checks ownership.

## R-005: NAuth Configuration for Site Requests

**Decision**: Keep `NAuth:JwtSecret` and `NAuth:BucketName` in the global NAuth section as defaults. The `ITenantSecretProvider` resolves per-tenant secrets from `Tenants:{tenantId}:JwtSecret`. The Site's Tenant field determines which tenant config to use.

**Rationale**: NAuth's `NAuthHandler` already uses `ITenantSecretProvider` to resolve JWT secrets per tenant. The middleware just needs to set the correct tenant in context before auth runs.
