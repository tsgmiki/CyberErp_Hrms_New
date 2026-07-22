import store from "@/store";
import { useSignals } from "@preact/signals-react/runtime";
import { useMemo } from "react";
import { useLocation } from "react-router-dom";

export interface FormPermissionState {
  finalDisable: boolean;
  disableValidate: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canView: boolean;
  canApprove: boolean;
}

export function useFormPermissions(
  voucherType?: string,
  currentStatus?: string,
): FormPermissionState {
  useSignals();
  const permissions = store.PermissionData.value;
  const workflows = store.WorkflowData.value;
  const pathName = useLocation().pathname;

  return useMemo(() => {
    // Match on a full path segment, NOT a raw substring — otherwise a sibling route whose link is a
    // prefix of another (e.g. "/loan" ⊂ "/loanType", "/trip" ⊂ "/tripBudget") would wrongly inherit
    // the shorter route's permissions and disable Save. Still matches nested routes ("/loan/123").
    const matchesRoute = (link?: string) => {
      if (!link) return false;
      return pathName === link || pathName.startsWith(`${link}/`);
    };

    const routePermission = permissions?.find((p) => matchesRoute(p.link as string));

    const blockedByRoute =
      typeof permissions !== "undefined" &&
      permissions.length > 0 &&
      permissions.some(
        (p) =>
          matchesRoute(p.link as string) &&
          (p.canAdd === false || p.canEdit === false),
      );

    const canAdd = Boolean(routePermission?.canAdd);
    const canEdit = Boolean(routePermission?.canEdit);
    const canView = Boolean(routePermission?.canView);
    const canApprove = Boolean(routePermission?.canApprove);

    const isSaveEnabled = (canAdd && canEdit) || (!canAdd && !canEdit);
    const finalDisable = blockedByRoute || !isSaveEnabled;

    const workflowFinalStatus =
      voucherType && workflows[voucherType]?.finalStatus;
    const isAtFinalStatus = workflowFinalStatus
      ? currentStatus === workflowFinalStatus
      : false;

    return {
      finalDisable,
      disableValidate: !canApprove && !isAtFinalStatus,
      canAdd,
      canEdit,
      canView,
      canApprove,
    };
  }, [permissions, pathName, voucherType, currentStatus, workflows]);
}
