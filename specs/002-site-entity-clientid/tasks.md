# Tasks: Site Entity with ClientId Account Separation

**Input**: Design documents from `/specs/002-site-entity-clientid/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Organization**: Tasks are grouped by user story to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: New project structures, shared database, and Site entity foundation

- [x] T001 Create `SiteStatus` enum in Peleja.Domain/Enums/SiteStatus.cs (Active=1, Blocked=2, Inactive=3)
- [x] T002 [P] Create `SiteModel` domain model in Peleja.Domain/Models/SiteModel.cs with rich methods (Create, Update, Activate, Deactivate, Block)
- [x] T003 [P] Create Site persistence entity in Peleja.Infra/Context/Site.cs
- [x] T004 Create `SharedContext` DbContext in Peleja.Infra/Context/SharedContext.cs with Site entity mapping (snake_case, indexes on client_id and site_url)
- [x] T005 [P] Create `SiteMapperProfile` in Peleja.Infra/Mappings/SiteMapperProfile.cs (Site <-> SiteModel)
- [x] T006 [P] Create `ISiteRepository<TModel>` in Peleja.Infra.Interfaces/Repositories/ISiteRepository.cs
- [x] T007 Create `SiteRepository` in Peleja.Infra/Repositories/SiteRepository.cs using SharedContext
- [x] T008 [P] Create DTOs: `SiteInsertInfo` in Peleja.DTO/SiteInsertInfo.cs, `SiteUpdateInfo` in Peleja.DTO/SiteUpdateInfo.cs, `SiteResult` in Peleja.DTO/SiteResult.cs
- [x] T009 [P] Create `SiteResultProfile` in Peleja.Domain/Mappings/SiteResultProfile.cs (SiteModel -> SiteResult)
- [x] T010 Add `ConnectionStrings:SharedContext` to all appsettings files (Development, Docker, Production)
- [x] T011 Register SharedContext, SiteRepository, and SiteService in Peleja.Application/DependencyInjection.cs
- [x] T012 Add shared DB connection string to docker-compose.yml, docker-compose-prod.yml, and .env.example/.env.prod.example
- [x] T013 Update peleja.sql with `sites` table DDL

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Middleware and Page entity changes that ALL user stories depend on

**CRITICAL**: No user story work can begin until this phase is complete

- [x] T014 Create `ClientIdMiddleware` in Peleja.API/Middleware/ClientIdMiddleware.cs — reads X-Client-Id header, looks up Site in SharedContext, validates status, sets TenantId (from Site.Tenant) and SiteId in HttpContext.Items
- [x] T015 Update `ITenantContext` in Peleja.Application/Interfaces/ITenantContext.cs — add SiteId property
- [x] T016 Update `TenantContext` in Peleja.Application/Services/TenantContext.cs — resolve SiteId from HttpContext.Items
- [x] T017 Update `TenantMiddleware` in Peleja.API/Middleware/TenantMiddleware.cs — keep for site admin endpoints only (X-Tenant-Id)
- [x] T018 Register ClientIdMiddleware in Peleja.API/Program.cs before auth middleware
- [x] T019 Remove `UserId` from `PageModel` in Peleja.Domain/Models/PageModel.cs, add `SiteId` field
- [x] T020 Remove `UserId` from `Page` in Peleja.Infra/Context/Page.cs, add `SiteId` field
- [x] T021 Update `PelejaContext` in Peleja.Infra/Context/PelejaContext.cs — update Page mapping (unique on site_id + page_url, remove user_id column)
- [x] T022 Update `PageMapperProfile` in Peleja.Infra/Mappings/PageMapperProfile.cs
- [x] T023 Update `PageRepository` in Peleja.Infra/Repositories/PageRepository.cs — queries by SiteId + PageUrl
- [x] T024 Update `IPageRepository<TModel>` in Peleja.Infra.Interfaces/Repositories/IPageRepository.cs — add GetByUrlAndSiteIdAsync method

**Checkpoint**: Foundation ready — Site entity, middleware, and Page changes complete

---

## Phase 3: User Story 1 — Register a Site (Priority: P1) MVP

**Goal**: Authenticated user registers a site and receives a unique ClientId

**Independent Test**: POST /api/v1/sites with JWT returns 201 with generated ClientId

### Implementation for User Story 1

- [x] T025 [US1] Create `SiteService` in Peleja.Domain/Services/SiteService.cs — CreateAsync generates ClientId (Guid.NewGuid "N" format), validates unique URL, persists via ISiteRepository
- [x] T026 [US1] Create `SiteController` in Peleja.API/Controllers/SiteController.cs — POST /api/v1/sites endpoint (requires auth, uses IUserClient for UserId)
- [x] T027 [US1] Add SiteController tests in Peleja.Tests/Domain/Services/SiteServiceTests.cs
- [x] T028 [US1] Add Site endpoints to Bruno collection in bruno/Sites/

**Checkpoint**: Site registration works. A user can create a site and receive a ClientId.

---

## Phase 4: User Story 2 — List Comments Using ClientId (Priority: P1)

**Goal**: Widget fetches comments using X-Client-Id header instead of X-Tenant-Id

**Independent Test**: GET /api/v1/comments with X-Client-Id header and pageUrl returns paginated comments from the correct tenant database

### Implementation for User Story 2

- [x] T029 [US2] Update `CommentService.GetByPageUrlAsync` in Peleja.Domain/Services/CommentService.cs — receive siteId parameter, pass to PageRepository
- [x] T030 [US2] Update `CommentController.GetComments` in Peleja.API/Controllers/CommentController.cs — read SiteId from HttpContext.Items (set by ClientIdMiddleware), remove X-Tenant-Id dependency
- [x] T031 [US2] Update CommentServiceTests in Peleja.Tests/Domain/Services/CommentServiceTests.cs
- [x] T032 [US2] Update CommentRepositoryTests in Peleja.Tests/Infra/Repositories/CommentRepositoryTests.cs
- [x] T033 [US2] Update Bruno Comments collection to use X-Client-Id header in bruno/Comments/

**Checkpoint**: Comment listing works via X-Client-Id. Tenant resolved automatically from Site.

---

## Phase 5: User Story 3 — Create Comments Under a Site (Priority: P1)

**Goal**: Authenticated user creates comments associated with a site via X-Client-Id

**Independent Test**: POST /api/v1/comments with X-Client-Id and Bearer token creates a comment in the correct tenant DB

### Implementation for User Story 3

- [x] T034 [US3] Update `CommentService.CreateAsync` in Peleja.Domain/Services/CommentService.cs — receive siteId, find/create Page by SiteId + PageUrl
- [x] T035 [US3] Update `CommentController.CreateComment` in Peleja.API/Controllers/CommentController.cs — read SiteId from context
- [x] T036 [P] [US3] Update `CommentController.UpdateComment` and `DeleteComment` in Peleja.API/Controllers/CommentController.cs — use SiteId context
- [x] T037 [P] [US3] Update `CommentLikeController.ToggleLike` in Peleja.API/Controllers/CommentLikeController.cs — use SiteId context
- [x] T038 [P] [US3] Update `GiphyController` in Peleja.API/Controllers/GiphyController.cs — use X-Client-Id
- [x] T039 [US3] Update CommentServiceTests for new siteId parameter in Peleja.Tests/Domain/Services/CommentServiceTests.cs
- [x] T040 [US3] Update CommentLikeServiceTests in Peleja.Tests/Domain/Services/CommentLikeServiceTests.cs

**Checkpoint**: All comment CRUD operations work via X-Client-Id.

---

## Phase 6: User Story 4 — Site Administration (Priority: P2)

**Goal**: Site owner can list, update, and moderate their sites. Site admin can delete any comment on their site.

**Independent Test**: GET /api/v1/sites lists owned sites. DELETE /api/v1/comments/{id} succeeds for site admin.

### Implementation for User Story 4

- [x] T041 [US4] Add GET /api/v1/sites (list user's sites) to `SiteController` in Peleja.API/Controllers/SiteController.cs
- [x] T042 [US4] Add PUT /api/v1/sites/{siteId} (update site) to `SiteController` in Peleja.API/Controllers/SiteController.cs
- [x] T043 [US4] Add ListByUserIdAsync and UpdateAsync to `SiteService` in Peleja.Domain/Services/SiteService.cs
- [x] T044 [US4] Update `CommentService.DeleteAsync` in Peleja.Domain/Services/CommentService.cs — add site admin check (Site.UserId == currentUserId)
- [x] T045 [US4] Add site admin moderation tests in Peleja.Tests/Domain/Services/CommentServiceTests.cs
- [x] T046 [US4] Add SiteService list/update tests in Peleja.Tests/Domain/Services/SiteServiceTests.cs
- [x] T047 [US4] Update Bruno Sites collection with GET and PUT in bruno/Sites/

**Checkpoint**: Site administration and comment moderation by site admin work.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, cleanup, and integration tests

- [x] T048 [P] Update API documentation in docs/api/README.md — add Site endpoints, update comment endpoints for X-Client-Id
- [x] T049 [P] Create docs/api/site-controller.md
- [x] T050 [P] Update docs/api/authentication.md — document X-Client-Id flow and site admin moderation
- [x] T051 [P] Update docs/api/comment-controller.md — X-Client-Id replaces X-Tenant-Id
- [x] T052 Update peleja.sql with final schema (sites table + page modifications)
- [x] T053 Update README.md with Site entity documentation
- [x] T054 Update Peleja.Tests.API integration tests for X-Client-Id header
- [x] T055 Update appsettings.Testing.Example.json with shared DB connection string

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies — can start immediately
- **Phase 2 (Foundational)**: Depends on Phase 1 — BLOCKS all user stories
- **Phase 3 (US1 - Register Site)**: Depends on Phase 2
- **Phase 4 (US2 - List Comments)**: Depends on Phase 2 (can run parallel with US1)
- **Phase 5 (US3 - Create Comments)**: Depends on Phase 2 (can run parallel with US1/US2)
- **Phase 6 (US4 - Admin)**: Depends on Phase 3 (needs SiteService)
- **Phase 7 (Polish)**: Depends on all user stories

### User Story Dependencies

- **US1 (Register Site)**: Foundational only — independent
- **US2 (List Comments)**: Foundational only — independent of US1
- **US3 (Create Comments)**: Foundational only — independent of US1/US2
- **US4 (Admin)**: Depends on US1 (SiteService) and integrates with US3 (delete permission)

### Within Each User Story

- Models before services
- Services before controllers
- Core implementation before tests
- Story complete before moving to next priority

### Parallel Opportunities

- T002, T003, T005, T006, T008, T009 can all run in parallel (Phase 1)
- US1, US2, US3 can start in parallel after Phase 2
- T036, T037, T038 can run in parallel (Phase 5)
- T048-T051 can all run in parallel (Phase 7)

---

## Parallel Example: Phase 1 Setup

```bash
# These can all run in parallel:
Task: "Create SiteModel in Peleja.Domain/Models/SiteModel.cs"
Task: "Create Site entity in Peleja.Infra/Context/Site.cs"
Task: "Create SiteMapperProfile in Peleja.Infra/Mappings/SiteMapperProfile.cs"
Task: "Create ISiteRepository in Peleja.Infra.Interfaces/Repositories/ISiteRepository.cs"
Task: "Create DTOs in Peleja.DTO/"
Task: "Create SiteResultProfile in Peleja.Domain/Mappings/SiteResultProfile.cs"
```

---

## Implementation Strategy

### MVP First (US1 + US2 + US3)

1. Complete Phase 1: Setup (Site entity, SharedContext, DTOs)
2. Complete Phase 2: Foundational (middleware, Page changes)
3. Complete Phase 3: US1 — Register Site (creates sites with ClientId)
4. Complete Phase 4: US2 — List Comments via ClientId
5. Complete Phase 5: US3 — Create Comments via ClientId
6. **STOP and VALIDATE**: Full comment widget flow works with X-Client-Id
7. Deploy/demo MVP

### Incremental Delivery

1. Setup + Foundational → Infrastructure ready
2. US1 → Site registration works → Demo
3. US2 + US3 → Comment widget fully functional with ClientId → Deploy MVP
4. US4 → Site admin and moderation → Deploy
5. Polish → Documentation and integration tests → Final release

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Use `/dotnet-architecture` skill when creating new entities
