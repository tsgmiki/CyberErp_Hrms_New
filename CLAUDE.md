# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository layout

Monorepo for an HRMS (multi-tenant SaaS). Two independently-built halves:

- `backend/` — ASP.NET Core **.NET 10** API, Clean Architecture across 4 projects. Solution: `CyberErp.Hrms.slnx`.
- `frontend/` — **React 19 + Vite** web SPA (`cyber-hrms-web`), TypeScript, Tailwind CSS v4.

> **Stale docs — do not trust for stack facts.** `frontend/README-TAURI.md` describes a Tauri desktop app, but there is no `src-tauri/`; it's now a plain web SPA. `backend/CyberErp.Hrms.Api/.github/copilot-instructions.md` calls this a "Business Book" system on Postgres — the actual DB is **SQL Server** and the domain is HRMS. Prefer this file and the code over those two.

## Commands

### Backend (run from `backend/`)
- Build: `dotnet build CyberErp.Hrms.slnx`
- Run API: `dotnet run --project CyberErp.Hrms.Api` → http://localhost:5014 (https https://localhost:7013). Swagger in Development.
- EF migrations (migrations live in the **Inf** project, startup is **Api**):
  - Add: `dotnet ef migrations add <Name> -p CyberErp.Hrms.Inf -s CyberErp.Hrms.Api`
  - Apply: `dotnet ef database update -p CyberErp.Hrms.Inf -s CyberErp.Hrms.Api`
  - A design-time factory (`HrmsDbContextDesignTimeFactory` / `HrmsDbContextFactory`) supplies the context outside the host.
- `CyberErp.Hrms.Api/CyberErp.Hrms.Api.http` has ready-made auth/endpoint requests.

### Frontend (run from `frontend/`)
- Dev server: `npm run dev` (Vite)
- Build: `npm run build` (`tsc -b && vite build` — typecheck gates the build)
- Lint: `npm run lint` (flat ESLint config)
- Scaffold a CRUD entity: `npm run scaffold:entity <EntityName> <domain> [ApiResource]` — prints the full set of files/patterns to create (see frontend conventions below).

## Backend architecture

**Clean Architecture dependency flow:** `Dom` ← `App` ← `Inf`, with `Api` as the composition root.

- **`CyberErp.Hrms.Dom`** — entities, domain events, constants. No external deps. `BaseEntity` (in `Entities/`) is the aggregate base: `Guid Id`, `TenantId` (implements `ITenantEntity`), NodaTime `Instant CreatedAt/UpdatedAt`, a `byte[] RowVersion` optimistic-concurrency token, and a domain-events list. Timestamps use NodaTime `Instant`, **not** `DateTime`.
- **`CyberErp.Hrms.App`** — application logic as **vertical slices** under `Features/{Domain}/{Entity}/{Operation}/`. Each slice defines an interface `I{Operation}` + handler `{Operation}` (plain classes, primary-constructor DI — **no MediatR**), plus `DTOs/`. The *repository interface* for a slice also lives here (e.g. `Login/ILoginRepository.cs`); validation is FluentValidation, thrown as `ValidationException` from the handler. `Common/` holds shared DTOs (`GetAllRequest`, `PaginatedResponse`), the generic `IRepository<>`, and service abstractions.
- **`CyberErp.Hrms.Inf`** — EF Core implementation: `Models/HrmsDbContext`, repository implementations (`Repositories/`, incl. generic `Repository<>`), Finbuckle multi-tenancy, and EF migrations.
- **`CyberErp.Hrms.Api`** — controllers (`Controllers/`, versioned) and minimal-API endpoints (`Endpoints/`). `Program.cs` is a thin composition root chaining `AddHrms*` extension methods defined in `Configuration/`. Add new service wiring there, not inline in `Program.cs`.

**Wiring is manual.** New handlers register in `App/DependencyInjection.cs`; new repositories register in `Inf/DependencyInjection.cs`. There is no assembly-scanning auto-registration for these.

**Multi-tenancy (Finbuckle.MultiTenant).** `HrmsDbContext : MultiTenantDbContext` — tenant isolation is enforced by Finbuckle (`[MultiTenant]` entities), resolved per-request via `HybridTenantStrategy` + `DatabaseTenantStore`. Every tenant-scoped entity carries `TenantId`; do not bypass the context to query across tenants. **Schema/table naming (after the 2026-07 rename): HRMS tables live in the `dbo` schema with no underscore — `dbo.hrmsEmployee`, `dbo.hrmsPosition`, etc. (configs use `ToTable("hrmsX", "dbo")`).** Non-HRMS tables (`Core.CorePerson`, `Core.coreSalaryScale`, `Core.lupStep`, `Core.Tenant`, `Core.User`, `Core.LookUpCategory`, …) remain in the `Core` schema, which is still the DbContext default (`HasDefaultSchema("Core")`). The report/lookup **stored procedures kept their `Core.hrms_*` names**; only tables were renamed.

**Auth.** Cookie/session based. `BaseController` applies `[Authorize(AuthenticationSchemes = "Cookies")]` and routes as `api/v{version}/[controller]`. JWT config also exists (`JwtConfiguration`) but controllers gate on the cookie scheme.

**Conventions to preserve when adding entities:** `Guid`→`uniqueidentifier`, `Instant`/`DateTime`→`datetime2`, and `RowVersion`→`varbinary(8)` concurrency token are applied globally in `OnModelCreating` by reflection — new entities inherit this automatically via `BaseEntity`. Register `IEntityTypeConfiguration`s explicitly in `OnModelCreating`.

## Frontend architecture

- **Path alias:** `@` → `src` (configured in both `vite.config.ts` and `tsconfig`). Import as `@/...`.
- **Entity CRUD is templated.** `src/template/` provides the reusable machinery — build new admin modules from it rather than hand-rolling:
  - `useEntityCrudModule` (list↔form state), `useEntityList`, `EntityModuleShell`, `EntityListShell`, `EntityListView`.
  - `createPagedQuery<T>(resource)` builds the standard `?page&size&sort` GET service; `createEntityGetById` for single fetch.
  - Per-entity services live at `src/services/{domain}/{entity}/{getAll,getById,create,update,delete}.ts` where `getAll.ts` is just `createPagedQuery<Model>("Resource")`.
  - A component module is `src/components/{domain}/{entity}/{index,list,form}.tsx` (index = `useEntityCrudModule` + `EntityModuleShell`, with `list`/`form` lazy-loaded).
  - `npm run scaffold:entity` prints this exact file set for a given entity.
- **HTTP layer:** `src/utils/apiClient.ts` (`api.get/post/put/patch/delete`) wraps `fetch` with `credentials: "include"`. On `401` it fires the `AUTH_ERROR_EVENT` and redirects to `/login` (pass `skipAuthRedirect` for auth-status probes). Base URL comes from `VITE_API_BASE_URL`.
- **State:** React Context stores in `src/store/` (aggregated in `store/index.tsx`); **TanStack React Query** for server state; `@preact/signals-react` available. i18n via `i18next`/`react-i18next` with `src/locales/`.
- **Routing:** `src/routes/index.tsx` — all pages `lazy` + `memo`. Authenticated pages nest under `/` wrapped in `ProtectedRoute`; `/login`, `/register`, `/logout` are public.

## Environment / config

- Frontend needs `VITE_API_BASE_URL` (and CORS on the API allows it via `Cors:AllowedOrigins`).
- Backend connection string is `ConnectionStrings:DefaultConnection` in `appsettings*.json` (SQL Server). The `DatabaseProvider` key exists but the DbContext currently hardcodes `UseSqlServer`.
