# CommentLikeController API Reference

**Base Path**: `/api/v1/comments/{commentId}/like`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

All endpoints require the `X-Tenant-Id` header. The TenantMiddleware validates the header and resolves the tenant context before the request reaches the controller.

---

## POST /api/v1/comments/{commentId}/like

Toggles a like on a comment. If the user has already liked the comment, the like is removed. If the user has not liked the comment, a like is added.

**Authentication**: Required (`Authorization: Basic {token}`)

### Path Parameters

| Parameter   | Type | Description                      |
|-------------|------|----------------------------------|
| `commentId` | long | ID of the comment to like/unlike |

### Response 200 (Like added)

Returns a `CommentLikeResult` directly:

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

| Status | Description           |
|--------|-----------------------|
| 401    | Not authenticated     |
| 404    | Comment not found     |
| 500    | Internal server error |
