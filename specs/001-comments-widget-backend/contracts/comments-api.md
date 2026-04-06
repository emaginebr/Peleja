# API Contract: Comments

**Base Path**: `/api/v1/comments`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

---

## GET /api/v1/comments

Lista comentários de uma página com paginação por cursor.

**Auth**: Opcional (se autenticado, inclui `isLikedByUser` nos resultados)

**Query Parameters**:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageUrl` | string | Yes | — | URL da página |
| `sortBy` | string | No | `recent` | `recent` ou `popular` |
| `cursor` | string | No | — | Cursor da página anterior |
| `pageSize` | int | No | 15 | Itens por página (max 50) |

**Response 200**:

```json
{
  "sucesso": true,
  "mensagem": "Comentários listados com sucesso",
  "dados": {
    "items": [
      {
        "commentId": 42,
        "content": "Ótimo artigo! 🎉",
        "gifUrl": "https://media.giphy.com/...",
        "pageUrl": "https://site.com/blog/post-1",
        "isEdited": false,
        "isDeleted": false,
        "likeCount": 5,
        "isLikedByUser": true,
        "createdAt": "2026-04-05T10:30:00",
        "author": {
          "userId": 1,
          "displayName": "João Silva",
          "avatarUrl": "https://..."
        },
        "replies": [
          {
            "commentId": 43,
            "content": "Concordo! 👏",
            "gifUrl": null,
            "isEdited": false,
            "isDeleted": false,
            "likeCount": 2,
            "isLikedByUser": false,
            "createdAt": "2026-04-05T11:00:00",
            "author": {
              "userId": 2,
              "displayName": "Maria Santos",
              "avatarUrl": null
            }
          }
        ]
      }
    ],
    "nextCursor": "eyJjb21tZW50SWQiOjQyfQ==",
    "hasMore": true
  }
}
```

**Response para comentário deletado**:

```json
{
  "commentId": 44,
  "content": "[Comentário removido]",
  "gifUrl": null,
  "isEdited": false,
  "isDeleted": true,
  "likeCount": 0,
  "isLikedByUser": false,
  "createdAt": "2026-04-05T09:00:00",
  "author": null,
  "replies": [...]
}
```

---

## POST /api/v1/comments

Cria um novo comentário.

**Auth**: Required (`Authorization: Basic {token}`)

**Request Body**:

```json
{
  "pageUrl": "https://site.com/blog/post-1",
  "content": "Ótimo artigo! 🎉",
  "gifUrl": "https://media.giphy.com/media/abc123/giphy.gif",
  "parentCommentId": null
}
```

| Field | Type | Required | Constraints |
|-------|------|----------|-------------|
| `pageUrl` | string | Yes | Max 2000 chars |
| `content` | string | Yes | Max 2000 chars, min 1 char |
| `gifUrl` | string | No | Max 500 chars, valid URL |
| `parentCommentId` | long? | No | Must exist and be root comment |

**Response 201**:

```json
{
  "sucesso": true,
  "mensagem": "Comentário criado com sucesso",
  "dados": {
    "commentId": 45,
    "content": "Ótimo artigo! 🎉",
    "gifUrl": "https://media.giphy.com/media/abc123/giphy.gif",
    "pageUrl": "https://site.com/blog/post-1",
    "parentCommentId": null,
    "isEdited": false,
    "likeCount": 0,
    "createdAt": "2026-04-05T12:00:00",
    "author": {
      "userId": 1,
      "displayName": "João Silva",
      "avatarUrl": "https://..."
    }
  }
}
```

**Response 400** (validação):

```json
{
  "sucesso": false,
  "mensagem": "Dados inválidos",
  "erros": ["O conteúdo deve ter no máximo 2000 caracteres"]
}
```

**Response 401**: Não autenticado
**Response 429**: Rate limit excedido (header `Retry-After`)

---

## PUT /api/v1/comments/{commentId}

Edita um comentário existente. Apenas o autor pode editar.

**Auth**: Required

**Request Body**:

```json
{
  "content": "Conteúdo atualizado 📝",
  "gifUrl": "https://media.giphy.com/media/xyz789/giphy.gif"
}
```

**Response 200**:

```json
{
  "sucesso": true,
  "mensagem": "Comentário atualizado com sucesso",
  "dados": {
    "commentId": 45,
    "content": "Conteúdo atualizado 📝",
    "gifUrl": "https://media.giphy.com/media/xyz789/giphy.gif",
    "isEdited": true,
    "updatedAt": "2026-04-05T13:00:00"
  }
}
```

**Response 403**: Usuário não é o autor
**Response 404**: Comentário não encontrado

---

## DELETE /api/v1/comments/{commentId}

Soft delete de um comentário. Autor ou moderador do tenant.

**Auth**: Required

**Response 200**:

```json
{
  "sucesso": true,
  "mensagem": "Comentário excluído com sucesso"
}
```

**Response 403**: Usuário não é o autor nem moderador
**Response 404**: Comentário não encontrado
