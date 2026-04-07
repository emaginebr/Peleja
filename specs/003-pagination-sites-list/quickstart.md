# Quickstart: Admin Lists & Pagination

## Implementation Order

### Step 1: PageResult DTO + Profile
1. Create `PageResult` DTO
2. Create `PageResultProfile` mapping

### Step 2: Repository Changes
1. Add `GetByUserIdPaginatedAsync` to ISiteRepository/SiteRepository
2. Add `GetBySiteIdWithCommentsAsync` to IPageRepository/PageRepository

### Step 3: Service Changes
1. Update `SiteService.ListByUserIdAsync` → paginated
2. Create `PageService` with `GetBySiteIdAsync`
3. Add `GetByPageIdAuthenticatedAsync` to CommentService

### Step 4: Controller Changes
1. Update `SiteController.ListSites` → paginated with cursor/pageSize params
2. Add `GET /api/v1/sites/{siteId}/pages` to SiteController
3. Add `GET /api/v1/sites/{siteId}/pages/{pageId}/comments` to SiteController

### Step 5: Tests + Bruno
1. Unit tests for new/modified services
2. Update Bruno collection
3. API integration tests
