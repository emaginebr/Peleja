# CommentController API Reference

**Base Path**: `/api/v1/comments`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

All endpoints require the `X-Tenant-Id` header. The TenantMiddleware validates the header and resolves the tenant context before the request reaches the controller.

---

## GET /api/v1/comments

Lists comments for a given page URL with cursor-based pagination.

**Authentication**: Optional. If authenticated, the response includes `isLikedByUser` for each comment.

### Query Parameters

| Parameter  | Type   | Required | Default  | Description                          |
|------------|--------|----------|----------|--------------------------------------|
| `pageUrl`  | string | Yes      | --       | URL of the page to list comments for |
| `sortBy`   | string | No       | `recent` | Sort order: `recent` or `popular`    |
| `cursor`   | string | No       | --       | Cursor from a previous response      |
| `pageSize` | int    | No       | `15`     | Items per page (max 50)              |

### Response 200 (Success)

Returns a `PaginatedResult<CommentResult>` directly:

```json
{
  "items": [
    {
      "commentId": 42,
      "content": "Otimo artigo!",
      "gifUrl": "https://media.giphy.com/...",
      "pageUrl": "https://site.com/blog/post-1",
      "isEdited": false,
      "isDeleted": false,
      "likeCount": 5,
      "isLikedByUser": true,
      "createdAt": "2026-04-05T10:30:00",
      "parentCommentId": null,
      "author": {
        "userId": 1,
        "displayName": "Joao Silva",
        "avatarUrl": "https://..."
      },
      "replies": [
        {
          "commentId": 43,
          "content": "Concordo!",
          "gifUrl": null,
          "isEdited": false,
          "isDeleted": false,
          "likeCount": 2,
          "isLikedByUser": false,
          "createdAt": "2026-04-05T11:00:00",
          "parentCommentId": 42,
          "author": {
            "userId": 2,
            "displayName": "Maria Santos",
            "avatarUrl": null
          }
        }
      ]
    }
  ],
  "nextCursor": "41",
  "hasMore": true
}
```

**Deleted comments** are included in the response but with redacted content:

```json
{
  "commentId": 44,
  "content": "[Comentario removido]",
  "gifUrl": null,
  "isEdited": false,
  "isDeleted": true,
  "likeCount": 0,
  "isLikedByUser": false,
  "createdAt": "2026-04-05T09:00:00",
  "parentCommentId": null,
  "author": null,
  "replies": []
}
```

### Error Responses

| Status | Description                          |
|--------|--------------------------------------|
| 400    | Missing or empty `pageUrl` parameter |
| 500    | Internal server error                |

---

## POST /api/v1/comments

Creates a new comment or reply.

**Authentication**: Required (`Authorization: Basic {token}`)

### Request Body

```json
{
  "pageUrl": "https://site.com/blog/post-1",
  "content": "Otimo artigo!",
  "gifUrl": "https://media.giphy.com/media/abc123/giphy.gif",
  "parentCommentId": null
}
```

| Field              | Type   | Required | Constraints                                   |
|--------------------|--------|----------|-----------------------------------------------|
| `pageUrl`          | string | Yes      | Max 2000 characters                           |
| `content`          | string | Yes      | Min 1 character, max 2000 characters          |
| `gifUrl`           | string | No       | Max 500 characters                            |
| `parentCommentId`  | long?  | No       | Must reference an existing root-level comment |

### Response 201 (Created)

Returns the created `CommentResult` directly with a `Location` header:

```json
{
  "commentId": 45,
  "content": "Otimo artigo!",
  "gifUrl": "https://media.giphy.com/media/abc123/giphy.gif",
  "pageUrl": "https://site.com/blog/post-1",
  "parentCommentId": null,
  "isEdited": false,
  "isDeleted": false,
  "likeCount": 0,
  "isLikedByUser": false,
  "createdAt": "2026-04-05T12:00:00",
  "author": {
    "userId": 1,
    "displayName": "Joao Silva",
    "avatarUrl": "https://..."
  },
  "replies": null
}
```

### Error Responses

| Status | Description              |
|--------|--------------------------|
| 400    | Validation error         |
| 401    | Not authenticated        |
| 404    | Parent comment not found |
| 429    | Rate limit exceeded      |
| 500    | Internal server error    |

---

## PUT /api/v1/comments/{commentId}

Updates an existing comment. Only the original author can edit their comment.

**Authentication**: Required (`Authorization: Basic {token}`)

### Path Parameters

| Parameter   | Type | Description              |
|-------------|------|--------------------------|
| `commentId` | long | ID of the comment to edit|

### Request Body

```json
{
  "content": "Conteudo atualizado",
  "gifUrl": "https://media.giphy.com/media/xyz789/giphy.gif"
}
```

| Field    | Type   | Required | Constraints                             |
|----------|--------|----------|-----------------------------------------|
| `content`| string | Yes      | Min 1 character, max 2000 characters    |
| `gifUrl` | string | No       | Max 500 characters                      |

### Response 200 (Success)

Returns the updated `CommentResult` directly.

### Error Responses

| Status | Description            |
|--------|------------------------|
| 400    | Validation error       |
| 401    | Not authenticated      |
| 403    | User is not the author |
| 404    | Comment not found      |
| 500    | Internal server error  |

---

## DELETE /api/v1/comments/{commentId}

Soft-deletes a comment. The comment content is replaced with `[Comentario removido]` and the author information is removed. The comment author or a tenant moderator can delete a comment.

**Authentication**: Required (`Authorization: Basic {token}`)

### Path Parameters

| Parameter   | Type | Description                |
|-------------|------|----------------------------|
| `commentId` | long | ID of the comment to delete|

### Response 204 (No Content)

Empty response body on successful deletion.

### Error Responses

| Status | Description                                |
|--------|--------------------------------------------|
| 401    | Not authenticated                          |
| 403    | User is not the author and not a moderator |
| 404    | Comment not found                          |
| 500    | Internal server error                      |
