# CommentLikeController API Reference

**Base Path**: `/api/v1/comments/{commentId}/like`
**Required Header**: `X-Client-Id: {client_id}`

All like endpoints require the `X-Client-Id` header with the site's Client ID (GUID). The site must have an **Active** status for like operations.

---

## POST /api/v1/comments/{commentId}/like

Toggles a like on a comment. If the user has already liked the comment, the like is removed. If the user has not liked the comment, a like is added.

**Authentication**: Required (`Authorization: Bearer {token}`)

### Path Parameters

| Parameter   | Type | Description                      |
|-------------|------|----------------------------------|
| `commentId` | long | ID of the comment to like/unlike |

### Response 200 (Like added)

Returns a `CommentLikeResult`:

```json
{
  "commentId": 42,
  "likeCount": 6,
  "isLikedByUser": true
}
```

### Response 200 (Like removed)

```json
{
  "commentId": 42,
  "likeCount": 5,
  "isLikedByUser": false
}
```

### Error Responses

| Status | Description                |
|--------|----------------------------|
| 401    | Not authenticated          |
| 403    | Site is Inactive or Blocked |
| 404    | Comment not found          |
| 500    | Internal server error      |
