# Feature Specification: Site Entity with ClientId Account Separation

**Feature Branch**: `002-site-entity-clientid`  
**Created**: 2026-04-06  
**Status**: Draft  
**Input**: User description: "Add Site entity with ClientId for account separation, relate Site to Page, and require ClientId on comment queries"

## Clarifications

### Session 2026-04-06

- Q: How should ClientId be transmitted in requests? → A: Via HTTP header `X-Client-Id` on all requests.
- Q: Should the site administrator have comment moderation powers? → A: Yes, the site admin can delete any comment on their site (acts as moderator).
- Q: Can two different users register the same site URL? → A: No, Site URL is globally unique in the database.
- Q: Where is the tenant configured? → A: Each Site has a Tenant field. The tenant determines which database and NAuth configuration to use. The `X-Tenant-Id` header is no longer needed — the tenant is resolved from the Site via the `X-Client-Id` header.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Register a Site (Priority: P1)

An authenticated user registers a new site by providing a site URL and selecting a tenant. The system generates a unique ClientId for the site. This ClientId will be used in all subsequent comment widget integrations to identify which site the comments belong to and which tenant to use.

**Why this priority**: Without a registered site, no comments can be associated to any account. This is the foundation of the entire feature.

**Independent Test**: Can be fully tested by calling the site registration endpoint with a valid JWT, site URL, and tenant. Delivers a ClientId that can be used immediately in the comment widget.

**Acceptance Scenarios**:

1. **Given** an authenticated user, **When** they register a site with URL "https://myblog.com" and tenant "emagine", **Then** a new site is created with a unique ClientId, the user becomes the site administrator, the tenant is set, and the status is set to Active.
2. **Given** an authenticated user who already owns a site, **When** they register another site with a different URL, **Then** a second site is created with its own unique ClientId.
3. **Given** an authenticated user, **When** they try to register a site with a URL that is already registered in the database, **Then** the system rejects the request with an appropriate error.

---

### User Story 2 - List Comments Using ClientId (Priority: P1)

A visitor or widget loads comments for a page by providing the ClientId (via `X-Client-Id` header) and page URL. The system resolves the tenant from the site's configuration, connects to the correct database, and returns comments for that page.

**Why this priority**: This is the core read path for the comment widget. Without ClientId-based filtering, comments from different sites would be mixed.

**Independent Test**: Can be fully tested by calling the GET comments endpoint with a valid `X-Client-Id` header and pageUrl. Returns only comments belonging to that specific site.

**Acceptance Scenarios**:

1. **Given** a site with ClientId "abc-123" configured with tenant "emagine" that has comments on "https://myblog.com/post-1", **When** a visitor requests comments with header `X-Client-Id: abc-123` and pageUrl "https://myblog.com/post-1", **Then** the system resolves the tenant, connects to the correct database, and returns only comments for that page.
2. **Given** two different sites with different ClientIds, **When** a visitor requests comments with one ClientId, **Then** comments from the other site are never included.
3. **Given** a ClientId that does not exist, **When** a visitor requests comments, **Then** an empty result is returned.
4. **Given** a site with status Blocked or Inactive, **When** a visitor requests comments using that site's ClientId, **Then** the request is rejected with an appropriate error.
5. **Given** a request without the `X-Client-Id` header, **When** a visitor requests comments, **Then** the request is rejected with 400 Bad Request.

---

### User Story 3 - Create Comments Under a Site (Priority: P1)

An authenticated user posts a comment on a page. The comment is associated with the site identified by the `X-Client-Id` header. The system resolves the tenant from the site and uses the correct database. If the page does not exist yet under that site, it is automatically created.

**Why this priority**: Comment creation is the core write operation. Comments must be correctly associated with the site via ClientId.

**Independent Test**: Can be fully tested by posting a comment with a valid `X-Client-Id` header, JWT token, and page URL. The comment is persisted and returned in subsequent list queries for that ClientId.

**Acceptance Scenarios**:

1. **Given** an active site with ClientId "abc-123" on tenant "emagine", **When** an authenticated user creates a comment with header `X-Client-Id: abc-123` and pageUrl "https://myblog.com/post-1", **Then** the comment is created in the correct tenant database. If the page did not exist, it is auto-created under the site.
2. **Given** a site with status Blocked, **When** a user tries to create a comment with that site's ClientId, **Then** the request is rejected.
3. **Given** a valid ClientId, **When** an unauthenticated user tries to create a comment, **Then** the request returns 401 Unauthorized.

---

### User Story 4 - Site Administration (Priority: P2)

The site administrator (the user who registered the site) can view and manage their sites, including updating the site URL and changing the site status. The site administrator also acts as a moderator and can delete any comment on their site.

**Why this priority**: Administrative capabilities are important but secondary to the core read/write comment flow.

**Independent Test**: Can be fully tested by calling site management endpoints with the administrator's JWT. The administrator can list their sites, update a site URL, deactivate a site, and delete any comment on their site.

**Acceptance Scenarios**:

1. **Given** an authenticated site administrator, **When** they list their sites, **Then** all sites they own are returned with their ClientIds, tenants, and statuses.
2. **Given** an authenticated site administrator, **When** they deactivate a site, **Then** the site status changes to Inactive and new comments on that site are rejected.
3. **Given** a user who is not the site administrator, **When** they try to manage the site, **Then** the request is rejected with 403 Forbidden.
4. **Given** a site administrator, **When** they delete a comment on their site, **Then** the comment is soft-deleted, even if the administrator is not the comment author and is not a NAuth admin.

---

### Edge Cases

- What happens when a site URL changes after pages and comments already exist? Pages remain associated; only the site URL record updates.
- What happens when a site is blocked or deactivated? Existing comments remain visible (read-only for Inactive), but new comments and likes are rejected. Blocked sites reject all operations including reads.
- What happens if the same page URL exists under two different sites? Each site has its own Page records — they are independent.
- What happens when the `X-Client-Id` header is not provided on a comment query? The request is rejected with 400 Bad Request.
- What happens when a site admin deletes a comment? The comment is soft-deleted following the same pattern as author/admin deletions.
- What happens if the tenant configured on a site does not exist in the system? The request fails with an appropriate error indicating invalid tenant configuration.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide a Site entity with fields: ClientId (unique identifier), SiteUrl (globally unique), Tenant (tenant identifier), UserId (administrator from NAuth), CreatedAt, UpdatedAt, and Status (Active, Blocked, Inactive).
- **FR-002**: System MUST auto-generate a unique ClientId when a new site is registered.
- **FR-003**: System MUST allow a single NAuth user to own multiple sites.
- **FR-004**: System MUST enforce Site URL uniqueness globally in the database.
- **FR-005**: System MUST associate each Page with a Site. A site can have multiple pages. The Page entity no longer has a UserId field.
- **FR-006**: System MUST require the `X-Client-Id` header on all comment listing queries. The system resolves the site, its tenant, and connects to the correct database.
- **FR-007**: System MUST require the `X-Client-Id` header on comment creation, update, delete, and like operations. The tenant is resolved from the site's configuration.
- **FR-008**: System MUST reject comment creation and like operations on sites with status Blocked or Inactive.
- **FR-009**: System MUST allow reading existing comments on Inactive sites (read-only mode). Blocked sites reject all operations including reads.
- **FR-010**: System MUST ensure only the site administrator (the UserId who registered the site) can manage site settings.
- **FR-011**: System MUST allow site administrators to list their own sites.
- **FR-012**: System MUST allow site administrators to update their site URL and status.
- **FR-013**: System MUST allow the site administrator to delete any comment on their site, acting as a moderator for that site.
- **FR-014**: System MUST resolve the tenant from the Site's Tenant field when processing requests with `X-Client-Id`. The `X-Tenant-Id` header is no longer required on comment endpoints.

### Key Entities

- **Site**: Represents a registered website account. Has a unique ClientId (used by the widget via `X-Client-Id` header), SiteUrl (globally unique), Tenant (determines which database and NAuth config to use), administrator UserId (from NAuth), status (Active/Blocked/Inactive), and timestamps. One user can own many sites.
- **Page**: Represents a specific page within a site where comments are posted. Has a PageUrl and belongs to a Site. No longer has its own UserId. One site can have many pages.
- **Comment**: Remains associated with a Page (and therefore indirectly with a Site). The ClientId is used to resolve which site and page the comment belongs to.
- **CommentLike**: No structural changes. Operations are gated by the site's status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Site registration completes in under 2 seconds and returns a usable ClientId.
- **SC-002**: Comment listing with ClientId returns results with the same performance as the current implementation (no degradation).
- **SC-003**: Comments from one site are never visible when querying with a different site's ClientId (100% account isolation).
- **SC-004**: Blocked sites return zero results on any operation. Inactive sites allow reads but reject writes.
- **SC-005**: A site administrator can manage all their sites from a single authenticated session.
- **SC-006**: A site administrator can delete any comment on their site without being a NAuth admin.
- **SC-007**: The correct tenant database is used for every request, resolved automatically from the site's Tenant field.

## Assumptions

- The ClientId is a system-generated unique string (e.g., UUID/GUID), not user-defined.
- The UserId for the site administrator comes from NAuth's `UserInfo.UserId` — no local user table exists.
- The Site entity is stored in a shared/central database (not per-tenant), since it contains the tenant mapping. Pages, Comments, and CommentLikes remain in per-tenant databases.
- The comment widget on the frontend will pass the `X-Client-Id` header in all requests. The `X-Tenant-Id` header is no longer needed on comment endpoints.
- Site status management (Blocked) is reserved for system administrators or future admin tooling; the site owner can set Active/Inactive.
- The list of valid tenants is defined in the application configuration (appsettings). A site's Tenant field must reference a configured tenant.
