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
 */

/** `"/JobCategory/"` → `"jobcategory"`. Tolerates links stored with or without a leading slash. */
export const normalizeRoutePath = (value?: string): string =>
  (value ?? "").trim().replace(/^\/+/, "").replace(/\/+$/, "").toLowerCase();

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
