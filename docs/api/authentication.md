# Authentication and Site Resolution

This document describes the authentication pipeline and site resolution used by the Peleja API.

---

## Site Resolution via X-Client-Id

### ClientIdMiddleware

Comment, like, and giphy endpoints require the `X-Client-Id` header with a valid ClientId (assigned when a site is registered).

**Header**: `X-Client-Id: {client_id}`

The middleware:

1. Looks up the `Site` by `ClientId`.
2. Validates the site's `Status` (Active/Inactive/Blocked).
3. Resolves the `Tenant` from the site for NAuth JWT validation.
4. Stores `TenantId`, `SiteId`, and `SiteUserId` in `HttpContext.Items`.

If the site is **Blocked**, returns **403**. If `X-Client-Id` is missing or invalid, downstream endpoints return **400**.

### TenantMiddleware

Site admin endpoints (`/api/v1/sites`) use `X-Tenant-Id` header directly, since these requests may not have a `ClientId` yet (e.g., site registration).

---

## NAuth JWT Authentication

Authentication is handled through NAuth. The tenant's JWT secret is resolved via `ITenantSecretProvider` based on the tenant identified by the site or header.

### Authentication Header

```
Authorization: Bearer {jwt_token}
```

### Flow

1. **ClientIdMiddleware** resolves the site and tenant from `X-Client-Id` (or **TenantMiddleware** from `X-Tenant-Id`).
2. **NAuthHandler** calls `ITenantSecretProvider.GetJwtSecret(tenantId)` to get the tenant's secret.
3. The JWT signature is validated.
4. If valid, the user session is available via `IUserClient.GetUserInSession(HttpContext)`.

### User Session

| Property | Type | Description |
|----------|------|-------------|
| `UserId` | long | User ID from NAuth |
| `Name` | string | Display name |
| `Email` | string | Email |
| `IsAdmin` | bool | Admin privileges |
| `ImageUrl` | string | Avatar URL |

---

## Tenant Configuration

Tenants are configured in `appsettings.json`. The `Tenant` field on each Site determines which NAuth config to use.

```json
{
  "NAuth": {
    "ApiURL": "http://nauth-api:80",
    "JwtSecret": "default-secret",
    "BucketName": "Peleja"
  },
  "Tenants": {
    "emagine": {
      "JwtSecret": "emagine-specific-secret",
      "BucketName": "Peleja"
    }
  }
}
```

---

## Permissions

| Action | Allowed |
|--------|---------|
| Register site | Authenticated users (with `X-Tenant-Id`) |
| List/update site | Site owner only |
| List comments | Anyone (with valid `X-Client-Id`, site Active or Inactive) |
| Create/edit comment | Authenticated users (site must be Active) |
| Delete comment | Comment author, NAuth admin, or site owner |
| Like/unlike | Authenticated users (site must be Active) |
| Search GIFs | Authenticated users |

---

## Site Status Access Control

| Status     | GET (reads) | POST/PUT/DELETE (writes) |
|------------|-------------|--------------------------|
| `Active`   | Allowed     | Allowed                  |
| `Inactive` | Allowed     | 403 Forbidden            |
| `Blocked`  | 403 Forbidden | 403 Forbidden          |

---

## Rate Limiting

- **Write endpoints** (`POST *`): 30 requests per minute per IP.
- Exceeded: **429 Too Many Requests** with `Retry-After` header.

---

## Endpoint Summary

| Endpoint | Header | Auth |
|----------|--------|------|
| `POST /api/v1/sites` | `X-Tenant-Id` | Required |
| `GET /api/v1/sites` | `X-Tenant-Id` | Required |
| `PUT /api/v1/sites/{siteId}` | `X-Tenant-Id` | Required |
| `GET /api/v1/comments` | `X-Client-Id` | Optional |
| `POST /api/v1/comments` | `X-Client-Id` | Required |
| `PUT /api/v1/comments/{id}` | `X-Client-Id` | Required |
| `DELETE /api/v1/comments/{id}` | `X-Client-Id` | Required |
| `POST /api/v1/comments/{id}/like` | `X-Client-Id` | Required |
| `GET /api/v1/giphy/search` | `X-Client-Id` | Required |
