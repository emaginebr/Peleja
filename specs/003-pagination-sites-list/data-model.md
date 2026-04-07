# Data Model: Admin Lists & Pagination

No new tables. Changes to existing queries only.

## New DTO

### PageResult

| Field | Type | Description |
|-------|------|-------------|
| pageId | long | Page ID |
| pageUrl | string | Page URL |
| commentCount | int | Number of comments on this page |
| createdAt | DateTime | When the page was created |

## Modified Queries

### Site List (paginated)

Existing `GetByUserIdAsync` → new `GetByUserIdPaginatedAsync(userId, cursor, pageSize)` returning `pageSize + 1` items ordered by `CreatedAt DESC`.

### Page List (with comment count)

New `GetBySiteIdWithCommentsAsync(siteId, cursor, pageSize)` — returns pages where comment count > 0, ordered by most recent comment, includes comment count.

### Comment List by Page ID

Reuses existing `GetByPageIdAsync(pageId, sortBy, cursor, pageSize)` — already implemented.
