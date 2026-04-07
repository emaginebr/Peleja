# API Contracts: Site Endpoints

## Headers

| Header | Required | Description |
|--------|----------|-------------|
| `X-Client-Id` | Yes (comment endpoints) | Identifies the site for comment operations |
| `X-Tenant-Id` | Yes (site admin endpoints) | Tenant for authentication on admin endpoints |
| `Authorization` | Yes (write operations) | Bearer JWT token from NAuth |

---

## POST /api/v1/sites

Register a new site.

**Auth**: Required

**Request**:
```json
{
  "siteUrl": "https://myblog.com",
  "tenant": "emagine"
}
```

**Response 201**:
```json
{
  "siteId": 1,
  "clientId": "a1b2c3d4e5f6...",
  "siteUrl": "https://myblog.com",
  "tenant": "emagine",
  "userId": 1,
  "status": 1,
  "createdAt": "2026-04-06T12:00:00"
}
```

**Errors**: 400 (validation), 401 (not authenticated), 409 (URL already registered)

---

## GET /api/v1/sites

List sites owned by the authenticated user.

**Auth**: Required

**Response 200**:
```json
[
  {
    "siteId": 1,
    "clientId": "a1b2c3d4e5f6...",
    "siteUrl": "https://myblog.com",
    "tenant": "emagine",
    "status": 1,
    "createdAt": "2026-04-06T12:00:00"
  }
]
```

---

## PUT /api/v1/sites/{siteId}

Update a site. Only the site administrator can update.

**Auth**: Required (site owner)

**Request**:
```json
{
  "siteUrl": "https://newblog.com",
  "status": 3
}
```

**Response 200**: Updated site object.

**Errors**: 400 (validation), 401, 403 (not owner), 404, 409 (URL conflict)

---

## Comment Endpoints (modified)

All existing comment endpoints now require `X-Client-Id` header instead of `X-Tenant-Id`.

### GET /api/v1/comments

**Headers**: `X-Client-Id: {clientId}`
**Query**: `pageUrl` (required), `sortBy`, `cursor`, `pageSize`

### POST /api/v1/comments

**Headers**: `X-Client-Id: {clientId}`, `Authorization: Bearer {token}`
**Body**: `{ "pageUrl", "content", "gifUrl?", "parentCommentId?" }`

### PUT /api/v1/comments/{commentId}

**Headers**: `X-Client-Id: {clientId}`, `Authorization: Bearer {token}`

### DELETE /api/v1/comments/{commentId}

**Headers**: `X-Client-Id: {clientId}`, `Authorization: Bearer {token}`

Delete allowed for: comment author, NAuth admin, OR site administrator.

### POST /api/v1/comments/{commentId}/like

**Headers**: `X-Client-Id: {clientId}`, `Authorization: Bearer {token}`

### GET /api/v1/giphy/search

**Headers**: `X-Client-Id: {clientId}`, `Authorization: Bearer {token}`
