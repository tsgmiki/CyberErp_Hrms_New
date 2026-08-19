import { useMemo } from "react";
import { useLocation } from "react-router-dom";
import { useSignals } from "@preact/signals-react/runtime";
import store from "@/store";
import { findBestRouteMatch } from "@/utils/routeMatch";

/** Route-scoped permissions for list toolbar (export, column picker). */
export function useListPermissions() {
  useSignals();
  const pathName = useLocation().pathname;
  const permissions = store.PermissionData.value;

  return useMemo(() => {
    // No permission backend loaded → don't hide toolbar tools (export, column picker).
    // Once permissions are configured, honour the per-route canView flag.
    const hasPermissions = Array.isArray(permissions) && permissions.length > 0;
    // Segment match, not `includes`: a raw substring made "/loanType" pick up the "/loan" row
    // (likewise "/trip" ⊂ "/tripBudget"), so the wrong operation decided the toolbar. Still
    // resolves nested record URLs ("/loan/{guid}") to their owning operation.
    const match = findBestRouteMatch(pathName, permissions, (entry) =>
      entry.link ? String(entry.link) : undefined,
    );
    const canView = !hasPermissions || match?.canView === true;
    // Export is its OWN privilege (Core.TenantRolePermission.CanExport). It used to follow
    // canView, so the column existed in the database, was granted or revoked on the role
    // screen, and decided nothing.
    const canExport = !hasPermissions || match?.canExport === true;

    return {
      canExport,
      canConfigureColumns: canView,
      operation: match,
    };
  }, [permissions, pathName]);
}
