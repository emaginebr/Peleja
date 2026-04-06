# Data Model: Backend do Widget de Comentários

**Branch**: `001-comments-widget-backend`
**Date**: 2026-04-05

## Entities

### Tenant

Representa um website/aplicação que utiliza o widget de comentários.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `tenant_id` | `bigint` | PK, identity | Identificador único |
| `name` | `varchar(260)` | NOT NULL | Nome do tenant |
| `slug` | `varchar(260)` | NOT NULL, UNIQUE | Slug identificador (valor do header X-Tenant-Id) |
| `nauth_api_url` | `varchar(500)` | NOT NULL | URL da API NAuth deste tenant |
| `nauth_api_key` | `varchar(500)` | NOT NULL | API key do NAuth deste tenant |
| `is_active` | `boolean` | NOT NULL, DEFAULT true | Se o tenant está ativo |
| `created_at` | `timestamp without time zone` | NOT NULL | Data de criação |
| `updated_at` | `timestamp without time zone` | NULL | Data de última atualização |

**PK**: `tenants_pkey`
**Unique**: `ix_tenants_slug` on `slug`

---

### User

Usuário autenticado via NAuth, vinculado a um tenant.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `user_id` | `bigint` | PK, identity | Identificador único |
| `tenant_id` | `bigint` | FK, NOT NULL | Tenant ao qual pertence |
| `nauth_user_id` | `varchar(260)` | NOT NULL | ID externo do NAuth |
| `display_name` | `varchar(260)` | NOT NULL | Nome de exibição |
| `avatar_url` | `varchar(500)` | NULL | URL do avatar |
| `role` | `integer` | NOT NULL, DEFAULT 1 | 1=User, 2=Moderator |
| `created_at` | `timestamp without time zone` | NOT NULL | Data de criação |
| `updated_at` | `timestamp without time zone` | NULL | Data de última atualização |

**PK**: `users_pkey`
**FK**: `fk_tenant_user` → `tenants(tenant_id)` ON DELETE ClientSetNull
**Unique**: `ix_users_tenant_nauth` on `(tenant_id, nauth_user_id)`

---

### Comment

Comentário principal ou resposta, vinculado a uma página e tenant.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `comment_id` | `bigint` | PK, identity | Identificador único |
| `tenant_id` | `bigint` | FK, NOT NULL | Tenant ao qual pertence |
| `user_id` | `bigint` | FK, NOT NULL | Autor do comentário |
| `parent_comment_id` | `bigint` | FK, NULL | Comentário pai (NULL = root) |
| `page_url` | `varchar(2000)` | NOT NULL | URL da página onde foi criado |
| `content` | `varchar(2000)` | NOT NULL | Conteúdo do comentário (texto + emojis) |
| `gif_url` | `varchar(500)` | NULL | URL do GIF do Giphy |
| `like_count` | `integer` | NOT NULL, DEFAULT 0 | Contagem de likes (desnormalizado) |
| `is_edited` | `boolean` | NOT NULL, DEFAULT false | Se foi editado |
| `is_deleted` | `boolean` | NOT NULL, DEFAULT false | Soft delete flag |
| `created_at` | `timestamp without time zone` | NOT NULL | Data de criação |
| `updated_at` | `timestamp without time zone` | NULL | Data de última atualização |
| `deleted_at` | `timestamp without time zone` | NULL | Data de exclusão |

**PK**: `comments_pkey`
**FK**: `fk_tenant_comment` → `tenants(tenant_id)` ON DELETE ClientSetNull
**FK**: `fk_user_comment` → `users(user_id)` ON DELETE ClientSetNull
**FK**: `fk_comment_reply` → `comments(comment_id)` ON DELETE ClientSetNull
**Index**: `ix_comments_page_url` on `(tenant_id, page_url, is_deleted)` — listagem principal
**Index**: `ix_comments_popular` on `(tenant_id, page_url, like_count DESC, comment_id DESC)` WHERE `is_deleted = false AND parent_comment_id IS NULL` — ordenação por populares
**Index**: `ix_comments_recent` on `(tenant_id, page_url, comment_id DESC)` WHERE `is_deleted = false AND parent_comment_id IS NULL` — ordenação por recentes
**Index**: `ix_comments_parent` on `(parent_comment_id)` WHERE `parent_comment_id IS NOT NULL` — busca de respostas

---

### CommentLike

Registro de like de um usuário em um comentário. Garante unicidade.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `comment_like_id` | `bigint` | PK, identity | Identificador único |
| `comment_id` | `bigint` | FK, NOT NULL | Comentário curtido |
| `user_id` | `bigint` | FK, NOT NULL | Usuário que curtiu |
| `created_at` | `timestamp without time zone` | NOT NULL | Data do like |

**PK**: `comment_likes_pkey`
**FK**: `fk_comment_like` → `comments(comment_id)` ON DELETE ClientSetNull
**FK**: `fk_user_like` → `users(user_id)` ON DELETE ClientSetNull
**Unique**: `ix_comment_likes_unique` on `(comment_id, user_id)` — 1 like por usuário por comentário

## Relationships

```text
Tenant (1) ──── (N) User
Tenant (1) ──── (N) Comment
User   (1) ──── (N) Comment
User   (1) ──── (N) CommentLike
Comment(1) ──── (N) Comment       (parent → replies, 1 nível)
Comment(1) ──── (N) CommentLike
```

## Query Filters Globais (EF Core)

- **Tenant isolation**: Todas as queries filtram automaticamente por `tenant_id` do contexto atual
- **Soft delete**: Comments com `is_deleted = true` são excluídos das queries padrão (exceto quando explicitamente incluídos para exibir "comentário removido")

## Enum: UserRole

| Value | Name | Description |
|-------|------|-------------|
| 1 | User | Usuário padrão — pode criar, editar e excluir seus próprios comentários |
| 2 | Moderator | Moderador — pode excluir qualquer comentário do tenant |
