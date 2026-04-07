# Tasks: Admin Lists & Pagination

**Input**: Design documents from `/specs/003-pagination-sites-list/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/

**Organization**: Tasks grouped by user story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: New DTO and mapping profile shared by multiple stories

- [x] T001 [P] Create `PageResult` DTO in Peleja.DTO/PageResult.cs with fields: pageId, pageUrl, commentCount, createdAt
- [x] T002 [P] Create `PageResultProfile` in Peleja.Domain/Mappings/PageResultProfile.cs (PageModel → PageResult, ignore commentCount)

---

## Phase 2: User Story 1 — Paginated Site List (Priority: P1)

**Goal**: GET /api/v1/sites returns paginated results with 15 items per page

**Independent Test**: Call GET /api/v1/sites with auth, verify `items`, `nextCursor`, `hasMore` fields

### Implementation for User Story 1

- [x] T003 [US1] Add `GetByUserIdPaginatedAsync(long userId, long? cursor, int pageSize)` to `ISiteRepository<TModel>` in Peleja.Infra.Interfaces/Repositories/ISiteRepository.cs
- [x] T004 [US1] Implement `GetByUserIdPaginatedAsync` in Peleja.Infra/Repositories/SiteRepository.cs — cursor on SiteId DESC, returns pageSize+1
- [x] T005 [US1] Update `SiteService.ListByUserIdAsync` in Peleja.Domain/Services/SiteService.cs — accept cursor/pageSize, return `PaginatedResult<SiteResult>`
- [x] T006 [US1] Update `SiteController.ListSites` in Peleja.API/Controllers/SiteController.cs — add `cursor` and `pageSize` query params, return paginated response
- [x] T007 [US1] Add SiteService pagination tests in Peleja.Tests/Domain/Services/SiteServiceTests.cs
- [x] T008 [US1] Update Bruno Sites/List My Sites.bru with cursor and pageSize params

**Checkpoint**: GET /api/v1/sites returns paginated results.

---

## Phase 3: User Story 2 — List Pages by Site (Priority: P1)

**Goal**: GET /api/v1/sites/{siteId}/pages returns pages with comments, paginated

**Independent Test**: Call endpoint with auth, verify only pages with comments are returned with commentCount

### Implementation for User Story 2

- [x] T009 [US2] Add `GetBySiteIdWithCommentsAsync(long siteId, long? cursor, int pageSize)` to `IPageRepository<TModel>` in Peleja.Infra.Interfaces/Repositories/IPageRepository.cs
- [x] T010 [US2] Implement `GetBySiteIdWithCommentsAsync` in Peleja.Infra/Repositories/PageRepository.cs — join with Comments count > 0, return PageModel list with comment count
- [x] T011 [US2] Create `PageService` in Peleja.Domain/Services/PageService.cs — `GetBySiteIdAsync(siteId, cursor, pageSize)` returns `PaginatedResult<PageResult>`
- [x] T012 [US2] Register `PageService` in Peleja.Application/DependencyInjection.cs
- [x] T013 [US2] Add `GET /api/v1/sites/{siteId}/pages` endpoint to `SiteController` in Peleja.API/Controllers/SiteController.cs — auth required, site owner check
- [x] T014 [US2] Add PageService tests in Peleja.Tests/Domain/Services/PageServiceTests.cs
- [x] T015 [P] [US2] Create Bruno Sites/List Pages.bru

**Checkpoint**: Pages with comments listed for a site with pagination.

---

## Phase 4: User Story 3 — Comment List by Page ID (Priority: P1)

**Goal**: GET /api/v1/sites/{siteId}/pages/{pageId}/comments returns paginated comments, authenticated

**Independent Test**: Call endpoint with auth, verify paginated comments. Verify public endpoint still works.

### Implementation for User Story 3

- [x] T016 [US3] Add `GetByPageIdAuthenticatedAsync` method to `CommentService` in Peleja.Domain/Services/CommentService.cs — accepts pageId, siteId for ownership validation, cursor, pageSize, sortBy
- [x] T017 [US3] Add `GET /api/v1/sites/{siteId}/pages/{pageId}/comments` endpoint to `SiteController` in Peleja.API/Controllers/SiteController.cs — auth required, site owner check, validate page belongs to site
- [x] T018 [US3] Add CommentService authenticated list tests in Peleja.Tests/Domain/Services/CommentServiceTests.cs
- [x] T019 [P] [US3] Create Bruno Sites/List Comments By Page.bru
- [x] T020 [US3] Verify existing public endpoint GET /api/v1/comments?pageUrl= still works unchanged (regression check)

**Checkpoint**: Admin can list comments by page ID. Public endpoint unchanged.

---

## Phase 5: Polish & Cross-Cutting Concerns

- [x] T021 [P] Update docs/api/site-controller.md with new paginated endpoints
- [x] T022 [P] Update docs/api/README.md with new endpoints table
- [x] T023 Update README.md API Endpoints section

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies
- **Phase 2 (US1)**: Depends on Phase 1 (shared DTOs)
- **Phase 3 (US2)**: Depends on Phase 1; independent of US1
- **Phase 4 (US3)**: Depends on Phase 1; independent of US1/US2
- **Phase 5 (Polish)**: Depends on all stories

### User Story Dependencies

- **US1**: Independent (modifies existing SiteService)
- **US2**: Independent (new PageService + new endpoint)
- **US3**: Independent (new CommentService method + new endpoint)

### Parallel Opportunities

- T001, T002 can run in parallel (Phase 1)
- US1, US2, US3 can all start in parallel after Phase 1
- T015, T019 can run in parallel (Bruno files)
- T021, T022 can run in parallel (docs)

---

## Implementation Strategy

### MVP First

1. Phase 1: Setup (DTOs)
2. Phase 2: US1 — Paginated site list
3. **VALIDATE**: List sites works with pagination
4. Phase 3: US2 — Pages by site
5. Phase 4: US3 — Comments by page
6. Phase 5: Polish

### Parallel Strategy

After Phase 1, all three stories can be implemented in parallel since they touch different files.
