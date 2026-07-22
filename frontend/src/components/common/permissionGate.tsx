import { memo, useMemo } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useMenuModules } from "@/components/menu/hooks/useMenuModules";
import getAllOperation from "@/services/admin/operation/getAll";
import { parameterInitialData } from "@/constants/initialization";

const norm = (s?: string) => (s ?? "").replace(/^\/+/, "").toLowerCase();

/**
 * Route-level permission guard. Blocks direct navigation (e.g. typing the URL) to a menu
 * operation the current role has NOT been granted. Deny-by-default, mirroring the sidebar:
 * - `grantedSet` = links from the role-scoped menu feed (only CanView operations, head-office = all).
 * - `catalogSet` = every operation link that exists, so we can tell a "gated but not granted"
 *   page apart from a genuinely non-menu page (dashboard, surveyTake, …) — the latter passes through.
 * A blocked path redirects to /unauthorized. Fails open if the catalog can't be loaded (the
 * backend still enforces data-access on every request; this is a navigation guard, not the wall).
 */
function PermissionGate() {
  const { pathname } = useLocation();
  const { modules, isLoading: menuLoading } = useMenuModules();
  const { data: catalog, isLoading: catalogLoading, isError } = useQuery({
    queryKey: ["operationCatalog"],
    queryFn: () => getAllOperation({ ...parameterInitialData, take: 1000 }),
    staleTime: 10 * 60 * 1000,
  });

  const grantedSet = useMemo(() => {
    const s = new Set<string>();
    (modules ?? []).forEach((m) =>
      (m.operations ?? []).forEach((op) => {
        if (op.canView !== false && op.link) s.add(norm(op.link));
      }),
    );
    return s;
  }, [modules]);

  const catalogSet = useMemo(() => {
    const s = new Set<string>();
    (catalog?.data ?? []).forEach((op) => {
      if (op.link) s.add(norm(op.link));
    });
    return s;
  }, [catalog]);

  // PERFORMANCE: never block navigation on the catalog/menu fetches (both are cached per
  // session). While they load we render optimistically; as soon as they resolve, an
  // unauthorized path redirects. The backend remains the real enforcement layer.
  if (menuLoading || catalogLoading) return <Outlet />;

  const path = norm(pathname);
  const isGatedOperation = !isError && catalogSet.has(path);
  if (isGatedOperation && !grantedSet.has(path)) {
    return <Navigate to="/unauthorized" replace />;
  }
  return <Outlet />;
}

export default memo(PermissionGate);
