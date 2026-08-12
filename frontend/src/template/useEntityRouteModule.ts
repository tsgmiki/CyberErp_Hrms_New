import { useCallback } from "react";
import { useLocation, useNavigate, useParams } from "react-router-dom";

/**
 * The single path segment directly under `basePath`, or undefined at the base itself.
 * Matches on a SEGMENT boundary: base "/appraisal" must not swallow "/appraisalTemplate".
 */
function segmentAfter(basePath: string, pathname: string): string | undefined {
  const base = basePath.replace(/\/+$/, "");
  if (pathname === base) return undefined;
  if (!pathname.startsWith(`${base}/`)) return undefined;
  const rest = pathname.slice(base.length + 1);
  return rest ? rest.split("/")[0] : undefined;
}

/** URL segment that means "creating a new record". Declared as a static route, so it is matched
 *  ahead of `:id` and never reaches the GUID guard. */
export const NEW_SEGMENT = "new";

export interface EntityRouteModule {
  /** Record id, or "" for create — same contract as `useEntityCrudModule`. */
  id: string;
  setId: (next: string) => void;
  showForm: boolean;
  backHandler: () => void;
  addHandler: () => void;
  editHandler: (recordId: string) => void;
}

/**
 * URL-driven twin of `useEntityCrudModule`, returning the **identical shape** so a module migrates
 * by changing one import and one call. The difference is where the state lives:
 *
 *   `useEntityCrudModule`  — `id` / `showForm` are React state; the URL never changes.
 *   `useEntityRouteModule` — both are DERIVED from the URL, so a record is linkable, bookmarkable,
 *                            survives refresh, and browser Back walks list ↔ form.
 *
 *     /branch          → list        (index route, `seg` undefined)
 *     /branch/new      → empty form  (id = "")
 *     /branch/{guid}   → edit form   (id = guid)
 *
 * `basePath` is the route root, e.g. "/branch".
 */
export function useEntityRouteModule(basePath: string): EntityRouteModule {
  const { id: param } = useParams();
  const { pathname } = useLocation();
  const navigate = useNavigate();

  // The segment normally arrives as `:id`, but a route may match it STATICALLY — declaring
  // `<Route path="new">` alongside `<Route path=":id">` matches "new" first and leaves
  // `useParams().id` undefined. Reading it from the URL as a fallback means the hook behaves the same
  // however the route is declared; without it, `showForm` stayed false on /x/new and the Add button
  // silently did nothing while the list kept rendering.
  const seg = param ?? segmentAfter(basePath, pathname);

  const isNew = seg === NEW_SEGMENT;
  // "" is the legacy "no record" sentinel that forms and `createSaveService` (POST vs PUT) rely on.
  const id = !seg || isNew ? "" : seg;
  const showForm = Boolean(seg);

  const backHandler = useCallback(() => navigate(basePath), [navigate, basePath]);

  const addHandler = useCallback(
    () => navigate(`${basePath}/${NEW_SEGMENT}`),
    [navigate, basePath],
  );

  const editHandler = useCallback(
    (recordId: string) => navigate(`${basePath}/${recordId}`),
    [navigate, basePath],
  );

  // Forms call setId(savedId) to switch create → edit, and setId("") after a completed save.
  const setId = useCallback(
    (next: string) => {
      // BEHAVIOUR CHANGE (migrated modules only): legacy `setId("")` left you on a blank form
      // because `showForm` was independent state. With the record in the URL, staying on
      // "/branch/new" right after saving an existing record is incoherent — so this returns to the
      // list. A module that wants "save & add another" calls addHandler() instead.
      if (!next) navigate(basePath);
      // `replace` so Back from the freshly-created record skips the /new step it came from.
      else navigate(`${basePath}/${next}`, { replace: true });
    },
    [navigate, basePath],
  );

  return { id, setId, showForm, backHandler, addHandler, editHandler };
}
