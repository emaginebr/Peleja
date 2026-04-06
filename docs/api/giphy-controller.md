# GiphyController API Reference

**Base Path**: `/api/v1/giphy`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

All endpoints require the `X-Tenant-Id` header. The TenantMiddleware validates the header and resolves the tenant context before the request reaches the controller.

---

## GET /api/v1/giphy/search

Searches for GIFs on Giphy by a search term. This endpoint proxies requests to the Giphy API using server-side credentials, so the Giphy API key is never exposed to clients.

**Authentication**: Required (`Authorization: Basic {token}`)

### Query Parameters

| Parameter | Type   | Required | Default | Description                        |
|-----------|--------|----------|---------|------------------------------------|
| `q`       | string | Yes      | --      | Search term                        |
| `limit`   | int    | No       | `20`    | Number of results to return (max 50) |
| `offset`  | int    | No       | `0`     | Offset for pagination              |

### Response 200 (Results found)

Returns a `GiphySearchResult` directly:

```json
{
  "items": [
    {
      "id": "abc123",
      "title": "Happy Dance",
      "url": "https://media.giphy.com/media/abc123/giphy.gif",
      "previewUrl": "https://media.giphy.com/media/abc123/200w.gif",
      "width": 480,
      "height": 270
    }
  ],
  "totalCount": 150,
  "offset": 0,
  "limit": 20
}
```

### Response 200 (No results)

```json
{
  "items": [],
  "totalCount": 0,
  "offset": 0,
  "limit": 20
}
```

### Error Responses

| Status | Description               |
|--------|---------------------------|
| 400    | Missing `q` parameter     |
| 401    | Not authenticated         |
| 503    | Giphy service unavailable |
| 500    | Internal server error     |

**503 response** uses ProblemDetails format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.4",
  "title": "An error occurred while processing your request.",
  "status": 503,
  "detail": "Servico de GIFs temporariamente indisponivel. Tente novamente em alguns instantes."
}
```
