# SiteController API Reference

**Base Path**: `/api/v1/sites`
**Required Header**: `X-Tenant-Id: {tenant_id}`
**Authentication**: Required on all endpoints

The SiteController manages site registration, configuration, and provides admin access to pages and comments.

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

Lists sites owned by the authenticated user with cursor-based pagination.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `cursor` | string | No | -- | Cursor from previous response |
| `pageSize` | int | No | 15 | Items per page (clamped 1-50) |

### Response 200

```json
{
  "items": [
    {
      "siteId": 1,
      "clientId": "a1b2c3d4e5f67890abcdef1234567890",
      "siteUrl": "https://mysite.com",
      "tenant": "emagine",
      "userId": 1,
      "status": 1,
      "createdAt": "2026-04-06T12:00:00"
    }
  ],
  "nextCursor": "2",
  "hasMore": false
}
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

## GET /api/v1/sites/{siteId}/pages

Lists pages of a site that have at least one comment. Only the site owner can access. Cursor-based pagination.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `siteId` | long | ID of the site |

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `cursor` | string | No | -- | Cursor from previous response |
| `pageSize` | int | No | 15 | Items per page (clamped 1-50) |

### Response 200

```json
{
  "items": [
    {
      "pageId": 1,
      "pageUrl": "https://mysite.com/post-1",
      "commentCount": 15,
      "createdAt": "2026-04-06T12:00:00"
    }
  ],
  "nextCursor": "2",
  "hasMore": false
}
```

### Errors

| Status | Description |
|--------|-------------|
| 401 | Not authenticated |
| 403 | Not the site owner |
| 404 | Site not found |

---

## GET /api/v1/sites/{siteId}/pages/{pageId}/comments

Lists comments for a specific page. Only the site owner can access. Cursor-based pagination. This is a separate admin endpoint from the public widget comment list.

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `siteId` | long | ID of the site |
| `pageId` | long | ID of the page |

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `sortBy` | string | No | `recent` | Sort order: `recent` or `popular` |
| `cursor` | string | No | -- | Cursor from previous response |
| `pageSize` | int | No | 15 | Items per page (clamped 1-50) |

### Response 200

Returns `PaginatedResult<CommentResult>` (same format as the public comment list).

```json
{
  "items": [
    {
      "commentId": 42,
      "content": "Great article!",
      "userId": 1,
      "userName": "John Doe",
      "userImageUrl": "https://img.example.com/john.jpg",
      "likeCount": 5,
      "isLikedByUser": true,
      "createdAt": "2026-04-06T12:00:00",
      "replies": []
    }
  ],
  "nextCursor": "41",
  "hasMore": true
}
```

### Errors

| Status | Description |
|--------|-------------|
| 401 | Not authenticated |
| 403 | Not the site owner |
| 404 | Site or page not found |

---

## Status Values

| Value | Name | Description |
|-------|------|-------------|
| 1 | Active | Full access |
| 2 | Blocked | All access denied (system admin only) |
| 3 | Inactive | Read-only (site owner can set) |
