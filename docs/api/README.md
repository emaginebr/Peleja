# Peleja API Documentation

Peleja is a multi-tenant comments widget backend built with .NET 8. It provides RESTful endpoints for managing comments, likes, and GIF search across multiple tenant sites.

---

## API Endpoints

| Method   | Path                                    | Auth     | Description                        |
|----------|-----------------------------------------|----------|------------------------------------|
| `GET`    | `/api/v1/comments`                      | Optional | List comments for a page (paginated) |
| `POST`   | `/api/v1/comments`                      | Required | Create a new comment or reply      |
| `PUT`    | `/api/v1/comments/{commentId}`          | Required | Edit an existing comment           |
| `DELETE` | `/api/v1/comments/{commentId}`          | Required | Soft-delete a comment              |
| `POST`   | `/api/v1/comments/{commentId}/like`     | Required | Toggle like on a comment           |
| `GET`    | `/api/v1/giphy/search`                  | Required | Search for GIFs                    |

## Controller Documentation

- [CommentController](comment-controller.md) -- List, create, update, and delete comments
- [CommentLikeController](comment-like-controller.md) -- Toggle likes on comments
- [GiphyController](giphy-controller.md) -- Search for GIFs via Giphy
- [Authentication and Multi-Tenant Flow](authentication.md) -- Authentication, tenant resolution, roles, and rate limiting

---

## Authentication Flow

All requests must include the `X-Tenant-Id` header to identify the tenant. Authenticated endpoints additionally require the `Authorization: Basic {token}` header.

1. The client sends a request with `X-Tenant-Id: {tenant_slug}`.
2. The **TenantMiddleware** resolves the tenant and verifies it is active.
3. For protected endpoints, the **NAuthHandler** validates the `Authorization` header against the tenant's NAuth API.
4. If the user does not exist locally, a `User` record is auto-provisioned with the default `User` role.
5. The request proceeds to the controller with the tenant context and user identity established.

For full details, see [Authentication and Multi-Tenant Flow](authentication.md).

---

## Multi-Tenant Model

Each tenant represents an independent site or application that uses the Peleja comments widget. Tenants are identified by a unique slug passed in the `X-Tenant-Id` header.

- **Tenant isolation**: Comments, users, likes, and moderator assignments are scoped per tenant.
- **Per-tenant auth**: Each tenant configures its own NAuth API URL and API key.
- **Tenant activation**: Only active tenants (`is_active = true`) can receive API requests.

---

## Response Format

The API follows standard .NET conventions:

- **Success responses** return the data directly in the response body (no wrapper).
- **Error responses** use the [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) format (`application/problem+json`).

**Success example** (GET /api/v1/comments):

```json
{
  "items": [
    {
      "commentId": 42,
      "content": "Great article!",
      "likeCount": 5,
      "isLikedByUser": true,
      "author": { "userId": 1, "displayName": "Joao Silva" },
      "replies": []
    }
  ],
  "nextCursor": "41",
  "hasMore": true
}
```

**Error example** (ProblemDetails):

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "O conteudo deve ter no maximo 2000 caracteres"
}
```

---

## Quick Start

### List comments (no authentication required)

```bash
curl -X GET "https://api.example.com/api/v1/comments?pageUrl=https://site.com/blog/post-1&pageSize=10" \
  -H "X-Tenant-Id: my-site"
```

### Create a comment

```bash
curl -X POST "https://api.example.com/api/v1/comments" \
  -H "X-Tenant-Id: my-site" \
  -H "Authorization: Basic dXNlcjpwYXNz" \
  -H "Content-Type: application/json" \
  -d '{
    "pageUrl": "https://site.com/blog/post-1",
    "content": "Great article!",
    "gifUrl": null,
    "parentCommentId": null
  }'
```

### Toggle a like

```bash
curl -X POST "https://api.example.com/api/v1/comments/42/like" \
  -H "X-Tenant-Id: my-site" \
  -H "Authorization: Basic dXNlcjpwYXNz"
```

### Search for GIFs

```bash
curl -X GET "https://api.example.com/api/v1/giphy/search?q=celebration&limit=5" \
  -H "X-Tenant-Id: my-site" \
  -H "Authorization: Basic dXNlcjpwYXNz"
```

### Edit a comment

```bash
curl -X PUT "https://api.example.com/api/v1/comments/45" \
  -H "X-Tenant-Id: my-site" \
  -H "Authorization: Basic dXNlcjpwYXNz" \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Updated content",
    "gifUrl": null
  }'
```

### Delete a comment

```bash
curl -X DELETE "https://api.example.com/api/v1/comments/45" \
  -H "X-Tenant-Id: my-site" \
  -H "Authorization: Basic dXNlcjpwYXNz"
```
