import { useMemo } from "react";
import { useLocation } from "react-router-dom";
import { useSignals } from "@preact/signals-react/runtime";
import store from "@/store";

/** Route-scoped permissions for list toolbar (export, column picker). */
export function useListPermissions() {
  useSignals();
  const pathName = useLocation().pathname;
  const permissions = store.PermissionData.value;

  return useMemo(() => {
    // No permission backend loaded → don't hide toolbar tools (export, column picker).
    // Once permissions are configured, honour the per-route canView flag.
    const hasPermissions = Array.isArray(permissions) && permissions.length > 0;
    const match = permissions?.find(
      (entry) => entry.link && pathName.includes(String(entry.link)),
    );
    const canView = !hasPermissions || match?.canView === true;

    return {
      canExport: canView,
      canConfigureColumns: canView,
      operation: match,
    };
  }, [permissions, pathName]);
}
