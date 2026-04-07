# CommentController API Reference

**Base Path**: `/api/v1/comments`
**Required Header**: `X-Client-Id: {client_id}`

All comment endpoints require the `X-Client-Id` header with the site's Client ID (GUID). The `ClientIdMiddleware` resolves the site and its associated tenant, making the `SiteId` available in the request context. The site must have an **Active** status for write operations (POST, PUT, DELETE). Read operations (GET) are allowed for both Active and Inactive sites.

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
| `pageSize` | int    | No       | `15`     | Items per page (clamped 1-50)        |

### Response 200 (Success)

Returns a `PaginatedResult<CommentResult>`:

```json
{
  "items": [
    {
      "commentId": 42,
      "content": "Great article!",
      "gifUrl": "https://media.giphy.com/...",
      "isEdited": false,
      "isDeleted": false,
      "likeCount": 5,
      "isLikedByUser": true,
      "createdAt": "2026-04-05T10:30:00",
      "parentCommentId": null,
      "userId": 1,
      "replies": [
        {
          "commentId": 43,
          "content": "I agree!",
          "gifUrl": null,
          "isEdited": false,
          "isDeleted": false,
          "likeCount": 2,
          "isLikedByUser": false,
          "createdAt": "2026-04-05T11:00:00",
          "parentCommentId": 42,
          "userId": 2
        }
      ]
    }
  ],
  "nextCursor": "41",
  "hasMore": true
}
```

**Deleted comments** are included with redacted content:

```json
{
  "commentId": 44,
  "content": "[Comment removed]",
  "gifUrl": null,
  "isEdited": false,
  "isDeleted": true,
  "likeCount": 0,
  "isLikedByUser": false,
  "createdAt": "2026-04-05T09:00:00",
  "parentCommentId": null,
  "userId": 0,
  "replies": []
}
```

### Page Auto-Creation

When the first comment is posted on a `pageUrl`, a `Page` record is automatically created and associated with the current site via `SiteId`. The GET endpoint returns an empty result if no Page exists yet.

### Error Responses

| Status | Description                          |
|--------|--------------------------------------|
| 400    | Missing or empty `pageUrl` parameter |
| 403    | Site is Blocked                      |
| 500    | Internal server error                |

---

## POST /api/v1/comments

Creates a new comment or reply.

**Authentication**: Required (`Authorization: Bearer {token}`)

### Request Body

```json
{
  "pageUrl": "https://site.com/blog/post-1",
  "content": "Great article!",
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

Returns the created `CommentResult` with a `Location` header:

```json
{
  "commentId": 45,
  "content": "Great article!",
  "gifUrl": "https://media.giphy.com/media/abc123/giphy.gif",
  "parentCommentId": null,
  "isEdited": false,
  "isDeleted": false,
  "likeCount": 0,
  "isLikedByUser": false,
  "createdAt": "2026-04-05T12:00:00",
  "userId": 1,
  "replies": null
}
```

### Validation Rules

- Content is required and cannot exceed 2000 characters
- Page URL is required and cannot exceed 2000 characters
- GIF URL cannot exceed 500 characters
- Parent comment must be a root comment (cannot reply to a reply)
- Parent comment must belong to the same page

### Error Responses

| Status | Description              |
|--------|--------------------------|
| 400    | Validation error         |
| 401    | Not authenticated        |
| 403    | Site is Inactive or Blocked |
| 404    | Parent comment not found |
| 429    | Rate limit exceeded      |
| 500    | Internal server error    |

---

## PUT /api/v1/comments/{commentId}

Updates an existing comment. Only the original author can edit their comment.

**Authentication**: Required (`Authorization: Bearer {token}`)

### Path Parameters

| Parameter   | Type | Description              |
|-------------|------|--------------------------|
| `commentId` | long | ID of the comment to edit|

### Request Body

```json
{
  "content": "Updated content",
  "gifUrl": "https://media.giphy.com/media/xyz789/giphy.gif"
}
```

| Field    | Type   | Required | Constraints                             |
|----------|--------|----------|-----------------------------------------|
| `content`| string | Yes      | Min 1 character, max 2000 characters    |
| `gifUrl` | string | No       | Max 500 characters                      |

### Response 200 (Success)

Returns the updated `CommentResult`.

### Error Responses

| Status | Description                |
|--------|----------------------------|
| 400    | Validation error           |
| 401    | Not authenticated          |
| 403    | User is not the author, or site is Inactive/Blocked |
| 404    | Comment not found          |
| 500    | Internal server error      |

---

## DELETE /api/v1/comments/{commentId}

Soft-deletes a comment. The comment content is replaced with `[Comment removed]` and the userId is set to 0. The comment author, a NAuth admin, or the site admin (site owner) can delete a comment.

**Authentication**: Required (`Authorization: Bearer {token}`)

### Path Parameters

| Parameter   | Type | Description                |
|-------------|------|----------------------------|
| `commentId` | long | ID of the comment to delete|

### Response 204 (No Content)

Empty response body on successful deletion.

### Delete Permissions

| Role | Can Delete |
|------|------------|
| Comment author | Yes |
| NAuth admin (`IsAdmin = true`) | Yes |
| Site admin (user who registered the site) | Any comment on their site |

### Error Responses

| Status | Description                            |
|--------|----------------------------------------|
| 401    | Not authenticated                      |
| 403    | User is not the author, not admin, and not site admin; or site is Inactive/Blocked |
| 404    | Comment not found                      |
| 500    | Internal server error                  |
