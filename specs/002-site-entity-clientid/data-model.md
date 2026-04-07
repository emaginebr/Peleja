# Data Model: Site Entity

## Entities

### Site (shared database)

| Field | Type | Constraints |
|-------|------|-------------|
| site_id | bigint | PK, identity always |
| client_id | varchar(32) | NOT NULL, unique |
| site_url | varchar(2000) | NOT NULL, unique |
| tenant | varchar(100) | NOT NULL |
| user_id | bigint | NOT NULL |
| status | integer | NOT NULL, default 1 |
| created_at | timestamp without time zone | NOT NULL |
| updated_at | timestamp without time zone | nullable |

**Status values**: 1 = Active, 2 = Blocked, 3 = Inactive

**Indexes**:
- `ix_sites_client_id` — unique on `client_id`
- `ix_sites_site_url` — unique on `site_url`
- `ix_sites_user_id` — on `user_id`

### Page (per-tenant database) — MODIFIED

| Field | Type | Constraints |
|-------|------|-------------|
| page_id | bigint | PK, identity always |
| site_id | bigint | NOT NULL |
| page_url | varchar(2000) | NOT NULL |
| created_at | timestamp without time zone | NOT NULL |
| updated_at | timestamp without time zone | nullable |

**Changes**: Removed `user_id`. Added `site_id`. Page URL unique per site (not globally).

**Indexes**:
- `ix_pages_site_page_url` — unique on (`site_id`, `page_url`)

**Foreign Keys**:
- None to Site table (cross-database). `site_id` is stored but not enforced by FK.

### Comment (per-tenant database) — NO CHANGES

Existing structure. Related to Page via `page_id`.

### CommentLike (per-tenant database) — NO CHANGES

Existing structure. Related to Comment via `comment_id`.

## Relationships

```
Site (shared DB)          Page (tenant DB)         Comment (tenant DB)
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│ site_id (PK) │◄────────│ site_id      │         │ comment_id   │
│ client_id    │         │ page_id (PK) │◄────────│ page_id (FK) │
│ site_url     │         │ page_url     │         │ user_id      │
│ tenant       │         │ created_at   │         │ content      │
│ user_id      │         │ updated_at   │         │ ...          │
│ status       │         └──────────────┘         └──────────────┘
│ created_at   │
│ updated_at   │
└──────────────┘
```

## New DbContext

**SharedContext** — connects to the shared database for Site lookups.

| DbSet | Entity |
|-------|--------|
| Sites | Site |

Connection string: `ConnectionStrings:SharedContext`

## State Transitions

```
Active ──► Inactive (site owner)
Active ──► Blocked (system admin)
Inactive ──► Active (site owner)
Blocked ──► Active (system admin)
```

| State | Read Comments | Write Comments | Manage Site |
|-------|---------------|----------------|-------------|
| Active | Yes | Yes | Yes |
| Inactive | Yes | No | Yes |
| Blocked | No | No | System admin only |
