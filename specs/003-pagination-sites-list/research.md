# Research: Admin Lists & Pagination

No unknowns identified. All patterns already exist in the codebase:

## R-001: Pagination Pattern

**Decision**: Reuse existing `PaginatedResult<T>` with cursor-based pagination.

**Rationale**: The comment list already implements this pattern with `items`, `nextCursor`, `hasMore`. Consistent UX.

## R-002: Page Comment Count

**Decision**: Use EF Core subquery count (`Pages.Where(...).Select(p => new { p, CommentCount = p.Comments.Count() })`) to include comment count without N+1 queries.

**Rationale**: Single query, efficient for the expected scale.

## R-003: Endpoint Structure

**Decision**: Nest page and comment admin endpoints under `/api/v1/sites/{siteId}/pages` and `/api/v1/sites/{siteId}/pages/{pageId}/comments`.

**Rationale**: RESTful hierarchy reflects ownership. Site owner auth check applies naturally at the site level.
