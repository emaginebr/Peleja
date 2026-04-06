# API Contract: Giphy

**Base Path**: `/api/v1/giphy`
**Required Header**: `X-Tenant-Id: {tenant_slug}`

---

## GET /api/v1/giphy/search

Busca GIFs no Giphy por termo.

**Auth**: Required (`Authorization: Basic {token}`)

**Query Parameters**:

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `q` | string | Yes | — | Termo de busca |
| `limit` | int | No | 20 | Número de resultados (max 50) |
| `offset` | int | No | 0 | Offset para paginação |

**Response 200**:

```json
{
  "sucesso": true,
  "mensagem": "GIFs encontrados",
  "dados": {
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
}
```

**Response 200** (sem resultados):

```json
{
  "sucesso": true,
  "mensagem": "Nenhum GIF encontrado",
  "dados": {
    "items": [],
    "totalCount": 0,
    "offset": 0,
    "limit": 20
  }
}
```

**Response 400**: Parâmetro `q` ausente
**Response 401**: Não autenticado
**Response 503**: Serviço Giphy indisponível

```json
{
  "sucesso": false,
  "mensagem": "Serviço de GIFs temporariamente indisponível. Tente novamente em alguns instantes."
}
```
