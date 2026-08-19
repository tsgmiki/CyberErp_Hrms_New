/**
 * Route → permission matching, shared by every place that maps a URL onto a `coreOperation.Link`.
 *
 * Extracted from `formProvider/formPermissions.ts`, which already got this right; the other call
 * sites each rolled their own and each was wrong in a different way:
 *   - `permissionGate` / `globalSearch` matched the path EXACTLY, so a nested entity URL
 *     ("/branch/{guid}") was not recognised as belonging to any operation and slipped through
 *     the navigation guard entirely.
 *   - `useListPermissions` / `gridAction` used a raw `String.includes`, so "/loanType" wrongly
 *     inherited the "/loan" permission row (and "/trip" ⊂ "/tripBudget", …).
 *
 * The rule here is a full path SEGMENT match: "/loan" never matches "/loanType", but does match
 * "/loan/new" and "/loan/{guid}".
 *
 * It also resolves the SUBSYSTEM NAMESPACE. Operation links are stored namespaced by the subsystem
 * that owns them ("/hrms/branch"), because one shared catalogue serves every subsystem and the same
 * screen name recurs across them. Each subsystem SPA is served at the ROOT of its own origin and
 * declares its routes there ("/branch"), so the namespace is an addressing convention of the
 * CATALOGUE, never part of a URL. It is stripped here, on both sides of every comparison; links
 * stored without it keep working unchanged.
 */
import { OWN_SUBSYSTEM_ABBREVIATION } from "@/config/appConfig";

/** This application's namespace in the shared catalogue, e.g. "hrms" — see the note above. */
const SUBSYSTEM_NAMESPACE = OWN_SUBSYSTEM_ABBREVIATION.trim().toLowerCase();

/**
 * A stored catalogue link as an actual APP URL:
 * `"/hrms/branch"` → `"/branch"` · `"hrms/branch"` → `"/branch"` · `"/branch"` → `"/branch"` ·
 * `"/hrms"` → `"/"`.
 *
 * Use this wherever a stored link becomes something NAVIGABLE (an `href`, a `navigate()` target).
 * To COMPARE a link against a pathname use `matchesRoute` / `normalizeRoutePath` instead — they
 * strip the same namespace. Casing is preserved here: react-router matches paths case-sensitively,
 * so "/jobCategory" must not become "/jobcategory".
 */
export function toAppPath(link?: string): string {
  const segments = (link ?? "").trim().split("/").filter(Boolean);
  if (segments[0]?.toLowerCase() === SUBSYSTEM_NAMESPACE) segments.shift();
  return `/${segments.join("/")}`;
}

/**
 * `"/JobCategory/"` → `"jobcategory"`, and `"/hrms/JobCategory"` → `"jobcategory"` too.
 *
 * Tolerates links stored with or without a leading slash, and with or without this subsystem's
 * catalogue namespace — so a granted menu link ("/hrms/branch"), a catalogue link ("/hrms/branch")
 * and a browser pathname ("/branch/{guid}") all reduce to the same key and compare equal.
 */
export const normalizeRoutePath = (value?: string): string => {
  const path = (value ?? "").trim().replace(/^\/+/, "").replace(/\/+$/, "").toLowerCase();
  if (path === SUBSYSTEM_NAMESPACE) return "";
  return path.startsWith(`${SUBSYSTEM_NAMESPACE}/`)
    ? path.slice(SUBSYSTEM_NAMESPACE.length + 1)
    : path;
};

/** True when `pathname` IS `link`, or is nested beneath it on a segment boundary. */
export function matchesRoute(pathname: string, link?: string): boolean {
  const path = normalizeRoutePath(pathname);
  const target = normalizeRoutePath(link);
  return target !== "" && (path === target || path.startsWith(`${target}/`));
}

/**
 * The LONGEST matching link, so a more specific operation always wins over a shorter one that
 * happens to be its prefix (`/loanType/{guid}` resolves to `loanType`, never `loan`).
 * Returns the normalized link, or undefined when the path belongs to no operation.
 */
export function resolveRouteKey(
  pathname: string,
  links: Iterable<string | undefined>,
): string | undefined {
  let best: string | undefined;
  for (const raw of links) {
    const link = normalizeRoutePath(raw);
    if (!matchesRoute(pathname, link)) continue;
    if (best === undefined || link.length > best.length) best = link;
  }
  return best;
}

/** Same longest-wins rule, but returns the owning record (a permission row, a menu operation, …). */
export function findBestRouteMatch<T>(
  pathname: string,
  items: readonly T[] | undefined,
  getLink: (item: T) => string | undefined,
): T | undefined {
  let best: T | undefined;
  let bestLength = -1;
  for (const item of items ?? []) {
    const link = normalizeRoutePath(getLink(item));
    if (!matchesRoute(pathname, link)) continue;
    if (link.length > bestLength) {
      best = item;
      bestLength = link.length;
    }
  }
  return best;
}
