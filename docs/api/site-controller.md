# SiteController API Reference

**Base Path**: `/api/v1/sites`
**Required Header**: `X-Tenant-Id: {tenant_id}`
**Authentication**: Required on all endpoints

The SiteController manages site registration and configuration. Each site gets a unique `ClientId` used by the comment widget.

---

## POST /api/v1/sites

Registers a new site.

### Request Body

```json
{
  "siteUrl": "https://mysite.com",
  "tenant": "emagine"
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `siteUrl` | string | Yes | Max 2000 chars, globally unique |
| `tenant` | string | Yes | Must match a configured tenant |

### Response 201 (Created)

```json
{
  "siteId": 1,
  "clientId": "a1b2c3d4e5f67890abcdef1234567890",
  "siteUrl": "https://mysite.com",
  "tenant": "emagine",
  "userId": 1,
  "status": 1,
  "createdAt": "2026-04-06T12:00:00"
}
```

### Errors

| Status | Description |
|--------|-------------|
| 400 | Validation error |
| 401 | Not authenticated |
| 409 | URL already registered |

---

## GET /api/v1/sites

Lists all sites owned by the authenticated user.

### Response 200

```json
[
  {
    "siteId": 1,
    "clientId": "a1b2c3d4e5f67890abcdef1234567890",
    "siteUrl": "https://mysite.com",
    "tenant": "emagine",
    "userId": 1,
    "status": 1,
    "createdAt": "2026-04-06T12:00:00"
  }
]
```

---

## PUT /api/v1/sites/{siteId}

Updates a site. Only the site owner can update.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `siteId` | long | ID of the site |

### Request Body

```json
{
  "siteUrl": "https://newsite.com",
  "status": 3
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `siteUrl` | string | No | New URL (max 2000 chars) |
| `status` | int | No | 1=Active, 3=Inactive |

### Response 200

Returns the updated site.

### Errors

| Status | Description |
|--------|-------------|
| 400 | Validation error |
| 401 | Not authenticated |
| 403 | Not the site owner |
| 404 | Site not found |
| 409 | URL already registered |

---

## Status Values

| Value | Name | Description |
|-------|------|-------------|
| 1 | Active | Full access |
| 2 | Blocked | All access denied (system admin only) |
| 3 | Inactive | Read-only (site owner can set) |
