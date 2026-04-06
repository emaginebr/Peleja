# Quickstart: Backend do Widget de Comentários

**Branch**: `001-comments-widget-backend`

## Pré-requisitos

- .NET 8.0 SDK
- PostgreSQL (rodando localmente ou remoto)
- Conta NAuth configurada
- API Key do Giphy (https://developers.giphy.com/)

## Setup

### 1. Configurar variáveis de ambiente

```bash
# Connection string PostgreSQL
export ConnectionStrings__PelejaContext="Host=localhost;Database=peleja;Username=postgres;Password=postgres"

# Environment
export ASPNETCORE_ENVIRONMENT=Development

# Giphy
export GIPHY_API_KEY="your-giphy-api-key"
```

### 2. Restaurar dependências

```bash
dotnet restore
```

### 3. Criar banco de dados e aplicar migrations

```bash
dotnet ef database update --project Peleja.Infra --startup-project Peleja.API
```

### 4. Executar a API

```bash
dotnet run --project Peleja.API
```

A API estará disponível em `https://localhost:5001` com Swagger em `https://localhost:5001/swagger`.

## Testando

### Testes unitários

```bash
dotnet test Peleja.Domain.Tests
```

### Testes de API

Requer a API rodando em ambiente de teste:

```bash
dotnet test Peleja.API.Tests
```

## Fluxo básico de teste manual

### 1. Listar comentários (público)

```bash
curl -H "X-Tenant-Id: meu-site" \
  "https://localhost:5001/api/v1/comments?pageUrl=https://meu-site.com/blog/post-1&sortBy=recent"
```

### 2. Criar comentário (autenticado)

```bash
curl -X POST \
  -H "X-Tenant-Id: meu-site" \
  -H "Authorization: Basic {token}" \
  -H "Content-Type: application/json" \
  -d '{"pageUrl":"https://meu-site.com/blog/post-1","content":"Ótimo artigo! 🎉"}' \
  "https://localhost:5001/api/v1/comments"
```

### 3. Responder comentário

```bash
curl -X POST \
  -H "X-Tenant-Id: meu-site" \
  -H "Authorization: Basic {token}" \
  -H "Content-Type: application/json" \
  -d '{"pageUrl":"https://meu-site.com/blog/post-1","content":"Concordo!","parentCommentId":1}' \
  "https://localhost:5001/api/v1/comments"
```

### 4. Toggle like

```bash
curl -X POST \
  -H "X-Tenant-Id: meu-site" \
  -H "Authorization: Basic {token}" \
  "https://localhost:5001/api/v1/comments/1/like"
```

### 5. Buscar GIFs

```bash
curl -H "X-Tenant-Id: meu-site" \
  -H "Authorization: Basic {token}" \
  "https://localhost:5001/api/v1/giphy/search?q=feliz"
```
