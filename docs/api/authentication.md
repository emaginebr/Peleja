# Authentication and Multi-Tenant Flow

This document describes the authentication pipeline and multi-tenant architecture used by the Peleja API.

---

## Multi-Tenant Architecture

### TenantMiddleware

Every API request must include the `X-Tenant-Id` header with a valid tenant slug.

**Header**: `X-Tenant-Id: {tenant_slug}`

The middleware performs the following steps in order:

1. **Validates** that the `X-Tenant-Id` header is present in the request.
2. **Looks up** the tenant by slug in the database.
3. **Verifies** that the tenant is active (`is_active = true`).
4. **Propagates** a scoped `TenantContext` containing the resolved `tenant_id` for use by downstream services.

Error responses use the ProblemDetails format (`application/problem+json`):

If the header is missing:

```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Header X-Tenant-Id e obrigatorio"
}
```

If the tenant slug is invalid or the tenant is inactive:

```json
{
  "status": 400,
  "title": "Bad Request",
  "detail": "Tenant nao encontrado ou inativo"
}
```

Both cases return HTTP **400 Bad Request**.

### Tenant Isolation

Each tenant has its own:
- NAuth API URL and API key for authentication
- Set of users, comments, and likes scoped to that tenant
- Moderator assignments independent of other tenants

---

## NAuth Basic Authentication

Authentication is handled through NAuth, an external authentication service. Each tenant configures its own NAuth instance.

### Authentication Header

```
Authorization: Basic {token}
```

The `{token}` is a Base64-encoded credential provided by NAuth.

### Authentication Flow

1. The **TenantMiddleware** identifies the tenant via the `X-Tenant-Id` header.
2. The **NAuthHandler** reads the tenant's `nauth_api_url` and `nauth_api_key` from the resolved `TenantContext`.
3. The handler validates the `Authorization: Basic {token}` header against the tenant's NAuth API.
4. If the token is valid, the user identity is established with claims (`UserId`, `Role`).
5. If the token is invalid or missing on a protected endpoint, the request is rejected with **401 Unauthorized**.

### Claims

After successful authentication, the following claims are available on the `User` principal:

| Claim    | Type   | Description                      |
|----------|--------|----------------------------------|
| `UserId` | long   | Internal user ID within Peleja   |
| `Role`   | int    | User role (1 = User, 2 = Moderator) |

---

## Auto-Provisioning of Users

When a user authenticates for the first time against a given tenant, the system automatically creates a local `User` record using data returned by NAuth. This is called auto-provisioning.

- **Default role**: `User` (role = 1)
- **Provisioned data**: Display name, avatar URL, and external NAuth identifier
- **No manual registration** is required; the user record is created transparently on first login

---

## User Roles

| Role      | Value | Permissions                                                    |
|-----------|-------|----------------------------------------------------------------|
| User      | 1     | Create, edit, and delete own comments; like/unlike comments    |
| Moderator | 2     | All User permissions plus delete any comment within the tenant |

Role promotion from User to Moderator is performed through administrative configuration and is not exposed via the public API.

---

## Rate Limiting

The API enforces rate limiting on write operations to prevent abuse.

### Rules

- **Write endpoints** (`POST *`): 30 requests per minute per IP.
- When the limit is exceeded, the API returns **429 Too Many Requests** with a `Retry-After` header.

### 429 Response

```
HTTP/1.1 429 Too Many Requests
Retry-After: 30
```

Clients should respect the `Retry-After` header value (in seconds) before retrying the request.

---

## Endpoint Authentication Summary

| Endpoint                               | Authentication |
|----------------------------------------|----------------|
| `GET /api/v1/comments`                 | Optional       |
| `POST /api/v1/comments`               | Required       |
| `PUT /api/v1/comments/{commentId}`     | Required       |
| `DELETE /api/v1/comments/{commentId}`  | Required       |
| `POST /api/v1/comments/{commentId}/like` | Required     |
| `GET /api/v1/giphy/search`            | Required       |
