# Feature Specification: Admin Lists & Pagination

**Feature Branch**: `003-pagination-sites-list`  
**Created**: 2026-04-07  
**Status**: Draft  
**Input**: User description: "Add pagination to sites list, create PageController, create authenticated comment list by page ID"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Paginated Site List (Priority: P1)

A site administrator lists their registered sites with pagination. The list returns 15 items per page by default and supports cursor-based pagination for navigating through large numbers of sites.

**Why this priority**: The site list already exists but returns all sites at once. Adding pagination is essential for administrators with many sites.

**Independent Test**: Call GET /api/v1/sites with authentication. Verify the response includes paginated results with `items`, `nextCursor`, and `hasMore` fields.

**Acceptance Scenarios**:

1. **Given** an authenticated user with 20 sites, **When** they request the site list without a cursor, **Then** the first 15 sites are returned with `hasMore: true` and a `nextCursor`.
2. **Given** an authenticated user with 20 sites, **When** they request the site list with the cursor from a previous response, **Then** the next 5 sites are returned with `hasMore: false`.
3. **Given** an authenticated user with 3 sites, **When** they request the site list, **Then** all 3 sites are returned with `hasMore: false` and `nextCursor: null`.
4. **Given** an authenticated user with no sites, **When** they request the site list, **Then** an empty list is returned with `hasMore: false`.

---

### User Story 2 - List Pages by Site (Priority: P1)

A site administrator views the pages of a specific site that have at least one comment. This allows the administrator to see which pages of their site are actively receiving engagement.

**Why this priority**: Administrators need visibility into which pages have comment activity to moderate and manage their sites effectively.

**Independent Test**: Call GET /api/v1/sites/{siteId}/pages with authentication. Verify only pages with at least one comment are returned, paginated.

**Acceptance Scenarios**:

1. **Given** a site with 3 pages that have comments and 2 pages with no comments, **When** the site administrator requests the page list, **Then** only the 3 pages with comments are returned.
2. **Given** a site with no pages that have comments, **When** the administrator requests the page list, **Then** an empty list is returned.
3. **Given** a user who is not the site owner, **When** they request the page list for that site, **Then** the request is rejected with 403 Forbidden.
4. **Given** a site with 20 pages with comments, **When** the administrator requests the page list, **Then** the first 15 pages are returned with pagination metadata.

---

### User Story 3 - Authenticated Comment List by Page ID (Priority: P1)

A site administrator views all comments for a specific page by its ID. This is a separate endpoint from the existing public comment list (which uses `pageUrl` and `X-Client-Id`). This authenticated endpoint uses the page ID directly and requires authentication, allowing administrators to moderate comments.

**Why this priority**: Administrators need a direct way to view comments for a specific page by ID for moderation, separate from the public widget endpoint.

**Independent Test**: Call GET /api/v1/sites/{siteId}/pages/{pageId}/comments with authentication. Verify paginated comments are returned. Verify the existing public endpoint (GET /api/v1/comments?pageUrl=) continues to work unchanged.

**Acceptance Scenarios**:

1. **Given** a page with 20 comments, **When** the administrator requests comments by page ID, **Then** the first 15 comments are returned with pagination metadata.
2. **Given** a page with 20 comments, **When** the administrator requests with a cursor, **Then** the next batch of comments is returned.
3. **Given** a user who is not the site owner, **When** they request comments for that page, **Then** the request is rejected with 403 Forbidden.
4. **Given** the existing public endpoint GET /api/v1/comments?pageUrl=, **When** a visitor uses it with X-Client-Id, **Then** it continues to work exactly as before (no changes).

---

### Edge Cases

- What happens when the site ID does not exist? Returns 404 Not Found.
- What happens when the page ID does not belong to the specified site? Returns 404 Not Found.
- What happens when cursor is invalid or expired? Returns the first page of results (ignores invalid cursor).
- What happens when pageSize exceeds the maximum? It is clamped to the maximum (50).

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The site list endpoint (GET /api/v1/sites) MUST return paginated results with 15 items per page by default, using cursor-based pagination.
- **FR-002**: The site list response MUST include `items`, `nextCursor`, and `hasMore` fields (same format as the comment list).
- **FR-003**: The system MUST provide a new endpoint to list pages by site ID (GET /api/v1/sites/{siteId}/pages) that returns only pages with at least one comment.
- **FR-004**: The page list endpoint MUST be authenticated and restricted to the site owner.
- **FR-005**: The page list endpoint MUST return paginated results with 15 items per page by default.
- **FR-006**: The system MUST provide a new endpoint to list comments by page ID (GET /api/v1/sites/{siteId}/pages/{pageId}/comments) with cursor-based pagination.
- **FR-007**: The comment list by page ID endpoint MUST be authenticated and restricted to the site owner.
- **FR-008**: The existing public comment list endpoint (GET /api/v1/comments?pageUrl=) MUST remain unchanged and continue to work with X-Client-Id header.
- **FR-009**: All new paginated endpoints MUST support `pageSize` (clamped 1-50, default 15) and `cursor` query parameters.
- **FR-010**: Page list results MUST include the page URL, comment count, and creation date for each page.

### Key Entities

- **Site**: Existing entity. The list endpoint is enhanced with pagination.
- **Page**: Existing entity. New list endpoint filtered by site, showing only pages with comments. Includes comment count.
- **Comment**: Existing entity. New admin list endpoint by page ID with pagination.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Site list loads within 1 second for users with up to 100 sites.
- **SC-002**: Page list loads within 1 second for sites with up to 500 pages.
- **SC-003**: Comment list by page ID loads within 1 second for pages with up to 1000 comments.
- **SC-004**: All three new paginated endpoints return correct `hasMore` and `nextCursor` values enabling smooth infinite scroll.
- **SC-005**: The existing public comment list endpoint continues to function identically (zero regressions).

## Assumptions

- Cursor-based pagination follows the same pattern already used by the comment list endpoint (`PaginatedResult<T>`).
- Page list returns pages ordered by most recent comment activity (pages with newer comments first).
- The page list includes a comment count per page to help administrators prioritize moderation.
- All three endpoints use the `X-Tenant-Id` header for authentication (site admin endpoints).
- The `pageSize` parameter defaults to 15, clamped between 1 and 50, consistent with existing pagination behavior.
