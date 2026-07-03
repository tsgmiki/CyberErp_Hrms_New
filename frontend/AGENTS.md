# Cyber ERP Web — Template Guide

This repository is an **ERP admin shell template** meant to be forked or adapted for multiple products. Follow these conventions so new modules stay consistent and cheap to build.

## Stack

- **React 19** + **Vite** + **TypeScript**
- **TanStack Query** for server state
- **react-i18next** (`src/locales/en.json`, `am.json`)
- **Tailwind v4** + semantic tokens in `src/config/theme.css`
- **Backend-driven navigation** (modules/operations from API, not hard-coded menus)

## Folder layout

| Path | Purpose |
|------|---------|
| `src/pages/` | Thin route entrypoints; delegate to `src/components/{domain}/` |
| `src/components/{domain}/{entity}/` | Feature module: `index.tsx`, `list.tsx`, `form.tsx` |
| `src/components/common/` | Shared UI: tables, forms, layout, badges |
| `src/template/` | **Template API** — list/CRUD primitives (`@/template`) |
| `src/services/{domain}/{entity}/` | API calls: `getAll`, `getById`, `create`, `update`, `delete` |
| `src/models/` | TypeScript models |
| `src/routes/index.tsx` | Route table |
| `src/config/` | `theme.css`, `appConfig.ts` (env) |

## Adding a new entity (checklist)

1. **Model** — `src/models/{Entity}Model.ts` (+ export from `models/index` if used).
2. **Services** — under `src/services/.../`:
   - `getAll.ts` → `createPagedQuery<Entity>("ResourceName")` from `@/template`
   - `get.ts` (single record) → `createEntityGetById<Entity>("ResourceName")` when the endpoint is `GET /Resource/{id}`
   - `getById`, `create`, `update`, `delete` as needed
3. **Module folder** — `src/components/{domain}/{entity}/`:
   - `index.tsx` — `useEntityCrudModule` + `EntityModuleShell` + lazy list/form
   - `list.tsx` — `useEntityList` + `EntityListShell` (or `EntityListView` if no custom actions)
   - `form.tsx` — `FormProvider` + field models
4. **Route** — register in `src/routes/index.tsx`.
5. **Backend menu** — module/operation `link` must match route path for sidebar + permissions.
6. **i18n** — keys in `en.json` / `am.json` for title, labels, export strings.
7. **List key** — unique `listKey` (e.g. `"roles"`) for column persistence and export filenames.

## List screens (standard)

**Minimal (no row actions in columns):**

```tsx
<EntityListView
  listKey="widgets"
  listLabel="Widgets"
  queryKey="widgets"
  fetchPage={getAllWidget}
  deleteById={deleteWidget}
  columns={widgetColumns}
/>
```

**With edit/delete in columns** (most admin lists):

```tsx
const list = useEntityList({ queryKey: "roles", fetchPage: getAllRole, deleteById: deleteRole });
const columns = useMemo(() => [...], [editHandler, list.deleteRecord]);

return (
  <EntityListShell listKey="roles" listLabel="Roles" columns={columns} {...list} />
);
```

Built-in toolbar per list: **column selector** (persisted), **Excel/PDF export** (page + all filtered), **list/grid toggle**. See `useListPage` in `src/components/common/dataTableProvider/`.

**Rich reference:** `src/components/sales/salesOrder/list.tsx` (filters, status cards, checkboxes, `EntityListShell` + extra stats query).  
**Minimal reference:** `src/components/admin/role/roleList.tsx`.

**Scaffold CLI:** `npm run scaffold:entity -- Widget sales Widget`

## CRUD module shell (standard)

```tsx
const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

return (
  <EntityModuleShell
    title="Roles"
    showForm={showForm}
    onList={backHandler}
    onAdd={addHandler}
    form={<RoleForm id={id} setRoleId={setId} />}
    list={<RoleList editHandler={editHandler} />}
  />
);
```

**Drawer or split body:** pass `children` instead of `list` / `form` when the list stays mounted — e.g. `src/components/admin/operation/index.tsx`, `src/components/sales/salesOrder/index.tsx`.

## API services

- Prefer `createPagedQuery` + `createEntityGetById` + `api` from `@/utils/apiClient` (auth redirect, JSON handling).
- Standard list `getAll.ts` files use `createPagedQuery<Entity>("ResourceOrReport/SubPath")`; nested paths such as `Report/Sales` are supported.
- Use `appConfig.apiBaseUrl` only when `api` is not suitable.
- List export-all uses `fetchAllListRows` (cap 10k) — see `listFetchAll.ts`.
- **Bulk migration reference:** `scripts/migrate-getAll-to-paged-query.mjs` documents the endpoint → model mapping used to normalize services (do not re-run blindly over customized endpoints).

## Theming & UI

- Use semantic classes: `text-table-header`, `text-label`, `bg-secondary`, not raw grays.
- Surfaces: avoid `bg-muted/30` when `--muted` is a text token; use `bg-secondary` or color-mix patterns from `theme.css`.
- Reuse `EntityBadge`, `GridAction`, `InventoryLayout`, `FormProvider`.

## Permissions

- Route `link` in permission store gates list toolbar (export/columns). See `useListPermissions.ts`.
- Extend with explicit flags when backend supports them.

## Adapting for a new project

1. Copy repo; set `.env`: `VITE_API_BASE_URL`, optional `VITE_APP_NAME`, `VITE_DEFAULT_LOCALE`.
2. Trim domains you do not need (routes + components + services).
3. Adjust `theme.css` brand tokens.
4. Keep `src/template/` and `src/components/common/dataTableProvider/` — they are the shared kernel.
5. Document product-specific rules in `.cursor/rules/` if using Cursor.

## Do not

- Re-export `useListPage` from `listViewToolbar` (circular import).
- Freeze table columns in `DataTableProvider` local state — pass `displayColumns` from parent.
- Scatter `import.meta.env` — use `appConfig.ts`.
