# Peleja API Documentation

Peleja is a comments widget backend built with .NET 8. It provides RESTful endpoints for managing sites, comments, likes, and GIF search.

---

## API Endpoints

### Site Management

| Method   | Path                           | Header         | Auth     | Description                     |
|----------|--------------------------------|----------------|----------|---------------------------------|
| `POST`   | `/api/v1/sites`                | `X-Tenant-Id`  | Required | Register a new site             |
| `GET`    | `/api/v1/sites`                | `X-Tenant-Id`  | Required | List authenticated user's sites |
| `PUT`    | `/api/v1/sites/{siteId}`       | `X-Tenant-Id`  | Required | Update a site (owner only)      |

### Comments & Likes

| Method   | Path                                    | Header         | Auth     | Description                        |
|----------|-----------------------------------------|----------------|----------|------------------------------------|
| `GET`    | `/api/v1/comments`                      | `X-Client-Id`  | Optional | List comments for a page (paginated) |
| `POST`   | `/api/v1/comments`                      | `X-Client-Id`  | Required | Create a new comment or reply      |
| `PUT`    | `/api/v1/comments/{commentId}`          | `X-Client-Id`  | Required | Edit an existing comment           |
| `DELETE` | `/api/v1/comments/{commentId}`          | `X-Client-Id`  | Required | Soft-delete a comment              |
| `POST`   | `/api/v1/comments/{commentId}/like`     | `X-Client-Id`  | Required | Toggle like on a comment           |

### GIF Search

| Method   | Path                                    | Header         | Auth     | Description                        |
|----------|-----------------------------------------|----------------|----------|------------------------------------|
| `GET`    | `/api/v1/giphy/search`                  | `X-Client-Id`  | Required | Search for GIFs                    |

## Controller Documentation

- [SiteController](site-controller.md) -- Register, list, and update sites
- [CommentController](comment-controller.md) -- List, create, update, and delete comments
- [CommentLikeController](comment-like-controller.md) -- Toggle likes on comments
- [GiphyController](giphy-controller.md) -- Search for GIFs via Giphy
- [Authentication Flow](authentication.md) -- Authentication, site resolution, and rate limiting

---

## Authentication Flow

- **Site management endpoints** require `X-Tenant-Id` header (identifies which NAuth tenant to authenticate against).
- **Comment/like/giphy endpoints** require `X-Client-Id` header (identifies the site; the tenant is resolved from the site's `Tenant` field).

1. The client sends `X-Client-Id: {client_id}` (or `X-Tenant-Id` for site endpoints).
2. The **ClientIdMiddleware** looks up the Site by `ClientId`, validates its status, and resolves the tenant for NAuth.
3. The **NAuthHandler** validates the JWT token using the tenant's JWT secret.
4. The user session is established via `IUserClient.GetUserInSession()`.

For full details, see [Authentication Flow](authentication.md).

---

## Site & Tenant Model

- **Tenant**: An NAuth configuration (JwtSecret, BucketName). Configured in `appsettings.json` under `Tenants`.
- **Site**: A registered website. Has a unique `ClientId`, `SiteUrl`, `Tenant` (for NAuth config), `UserId` (owner), and `Status`.
- **Page**: A specific URL within a site where comments are posted.

All data is stored in a single PostgreSQL database.

### Site Status

| Status     | Reads | Writes | Description                              |
|------------|-------|--------|------------------------------------------|
| `Active`   | Yes   | Yes    | Full read and write access               |
| `Inactive` | Yes   | No     | Read-only; write requests return 403     |
| `Blocked`  | No    | No     | All requests return 403                  |

---

## Response Format

- **Success responses** return the data directly (no wrapper).
- **Error responses** use [RFC 7807 Problem Details](https://datatracker.ietf.org/doc/html/rfc7807) format.

---

## Quick Start

### Register a site

```bash
curl -X POST "http://localhost:5000/api/v1/sites" \
  -H "X-Tenant-Id: emagine" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{ "siteUrl": "https://mysite.com", "tenant": "emagine" }'
```

### List comments

```bash
curl -X GET "http://localhost:5000/api/v1/comments?pageUrl=https://mysite.com/post-1" \
  -H "X-Client-Id: {client_id}"
```

### Create a comment

```bash
curl -X POST "http://localhost:5000/api/v1/comments" \
  -H "X-Client-Id: {client_id}" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{ "pageUrl": "https://mysite.com/post-1", "content": "Great article!" }'
```
