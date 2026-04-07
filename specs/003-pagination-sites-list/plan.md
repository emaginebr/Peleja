# Implementation Plan: Admin Lists & Pagination

**Branch**: `003-pagination-sites-list` | **Date**: 2026-04-07 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-pagination-sites-list/spec.md`

## Summary

Add cursor-based pagination to the site list, create a PageController to list pages with comments by site, and create an authenticated comment list by page ID. All new endpoints are authenticated and restricted to site owners. The existing public comment endpoint remains unchanged.

## Technical Context

**Language/Version**: C# / .NET 8.0  
**Primary Dependencies**: Entity Framework Core, AutoMapper, NAuth 0.5.10  
**Storage**: PostgreSQL (single database, peleja_ prefixed tables)  
**Testing**: xUnit, Moq, FluentAssertions, Flurl.Http  
**Target Platform**: Linux Docker containers  
**Project Type**: Web service (REST API)  
**Performance Goals**: All list endpoints respond < 1 second  
**Constraints**: Reuse existing `PaginatedResult<T>` pattern  
**Scale/Scope**: Up to 100 sites, 500 pages, 1000 comments per page

## Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| .NET 8.0 + EF Core | PASS | Existing stack |
| PostgreSQL conventions (snake_case, peleja_ prefix) | PASS | No new tables |
| NAuth authentication | PASS | Site owner endpoints use X-Tenant-Id |
| AutoMapper for mapping | PASS | New PageResult DTO |
| File-scoped namespaces | PASS | Project convention |

No violations.

## Project Structure

### Source Code Changes

```text
Peleja.DTO/
├── PageResult.cs                      # NEW: Page DTO with comment count

Peleja.Domain/
├── Mappings/
│   └── PageResultProfile.cs           # NEW: PageModel → PageResult
├── Services/
│   ├── SiteService.cs                 # MODIFIED: paginated list
│   ├── PageService.cs                 # NEW: list pages by site with comment count
│   └── CommentService.cs             # MODIFIED: add GetByPageIdAsync (authenticated)

Peleja.Infra/
├── Repositories/
│   ├── SiteRepository.cs             # MODIFIED: paginated query
│   ├── PageRepository.cs             # MODIFIED: add GetBySiteIdWithCommentsAsync
│   └── CommentRepository.cs          # (existing GetByPageIdAsync already works)

Peleja.Infra.Interfaces/
├── Repositories/
│   ├── ISiteRepository.cs            # MODIFIED: paginated method
│   └── IPageRepository.cs            # MODIFIED: add new method

Peleja.API/
├── Controllers/
│   ├── SiteController.cs             # MODIFIED: paginated GET, add page/comment sub-routes
│   └── PageController.cs             # NEW: or nested in SiteController

Peleja.Tests/
├── Domain/Services/
│   ├── SiteServiceTests.cs           # NEW
│   ├── PageServiceTests.cs           # NEW
│   └── CommentServiceTests.cs        # MODIFIED

bruno/
├── Sites/                            # MODIFIED: update list request
```
