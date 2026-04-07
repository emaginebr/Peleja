# API Contracts: Admin List Endpoints

All endpoints require `X-Tenant-Id` header and `Authorization: Bearer {token}`. Restricted to site owner.

---

## GET /api/v1/sites (modified — now paginated)

**Query**: `cursor` (optional), `pageSize` (default 15, max 50)

**Response 200**:
```json
{
  "items": [
    {
      "siteId": 1,
      "clientId": "a1b2c3d4...",
      "siteUrl": "https://mysite.com",
      "tenant": "emagine",
      "userId": 1,
      "status": 1,
      "createdAt": "2026-04-06T12:00:00"
    }
  ],
  "nextCursor": "2",
  "hasMore": true
}
```

---

## GET /api/v1/sites/{siteId}/pages

Lists pages with comments for a site. Requires site owner auth.

**Query**: `cursor` (optional), `pageSize` (default 15, max 50)

**Response 200**:
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

**Errors**: 401, 403 (not site owner), 404 (site not found)

---

## GET /api/v1/sites/{siteId}/pages/{pageId}/comments

Lists comments for a page. Requires site owner auth. Cursor-based pagination.

**Query**: `sortBy` (recent/popular), `cursor` (optional), `pageSize` (default 15, max 50)

**Response 200**: Same `PaginatedResult<CommentResult>` format as existing public endpoint.

**Errors**: 401, 403 (not site owner), 404 (site or page not found)
