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
    const match = permissions?.find(
      (entry) => entry.link && pathName.includes(String(entry.link)),
    );
    const canView = match?.canView === true;

    return {
      canExport: canView,
      canConfigureColumns: canView,
      operation: match,
    };
  }, [permissions, pathName]);
}
