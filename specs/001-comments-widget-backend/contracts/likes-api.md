# API Contract: Likes

**Base Path**: `/api/v1/comments/{commentId}/like`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

---

## POST /api/v1/comments/{commentId}/like

Toggle de like em um comentário. Se o usuário já curtiu, remove o like. Se não curtiu, adiciona.

**Auth**: Required (`Authorization: Basic {token}`)

**Path Parameters**:

| Parameter | Type | Description |
|-----------|------|-------------|
| `commentId` | long | ID do comentário |

**Response 200** (like adicionado):

```json
{
  "sucesso": true,
  "mensagem": "Like adicionado",
  "dados": {
    "commentId": 42,
    "likeCount": 6,
    "isLikedByUser": true
  }
}
```

**Response 200** (like removido):

```json
{
  "sucesso": true,
  "mensagem": "Like removido",
  "dados": {
    "commentId": 42,
    "likeCount": 5,
    "isLikedByUser": false
  }
}
```

**Response 401**: Não autenticado
**Response 404**: Comentário não encontrado
