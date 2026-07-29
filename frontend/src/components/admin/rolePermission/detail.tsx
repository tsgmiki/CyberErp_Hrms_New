"use client";
import DataTableProvider from "../../common/dataTableProvider/dataTableProvider";
import { useQuery } from "@tanstack/react-query";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type {
  DataTableColumnModel,
  OperationModel,
  ParameterModel,
} from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import getAllRolePermission from "@/services/admin/rolePermission/getAll";
import getAllOperation from "@/services/admin/operation/getAll";
import {
  PermissionCheckbox,
  PermissionColumnHeader,
} from "./permissionCheckbox";
import {
  buildPermissionMap,
  setColumnAll,
  setPermissionField,
  toPermissionDetails,
  type PermissionField,
  type PermissionMap,
  type PermissionState,
} from "./rolePermissionUtils";
import type { MenuScope } from "@/components/common/menuFilters/subsystemModuleFilter";

interface RolePermissionDetailProps {
  roleId: string;
  scope?: MenuScope;
  editHandler: (permissions: PermissionState[]) => void;
}

interface OperationItem {
  id: string;
  name: string;
  moduleId: string;
  module: string;
  subsystemId: string;
  subSystem: string;
}

const PERMISSION_COLUMNS: { field: PermissionField; label: string }[] = [
  { field: "canView", label: "View" },
  { field: "canAdd", label: "Add" },
  { field: "canEdit", label: "Edit" },
  { field: "canDelete", label: "Delete" },
  { field: "canApprove", label: "Approve" },
];

/**
 * The role × operation permission matrix. Deliberately simple and self-contained:
 * - FLAT table (no collapsible grouping — nothing can ever hide the rows), sorted Module → Operation.
 * - Permissions state is a stable map keyed by operationId; toggling never reorders anything.
 * - Server rows are filtered by roleId CLIENT-SIDE too, so a backend that ignores the role filter
 *   can never bleed another role's grants into this matrix.
 * - Initialized once per role (ref-guarded) — background refetches never wipe in-progress edits.
 * - The Subsystem → Module scope only narrows the VISIBLE rows (and what check-all touches);
 *   the permission map always covers ALL operations, so saving while filtered never drops
 *   grants that were ticked outside the current filter.
 */
function RolePermissionDetail({ roleId, scope, editHandler }: RolePermissionDetailProps) {
  const [permissions, setPermissions] = useState<PermissionMap>({});
  const loadedRoleRef = useRef<string | null>(null);

  const [listParam] = useState<ParameterModel>({
    ...parameterInitialData,
    take: 1000,
  });

  const { data: operations, isLoading: operationsLoading } = useQuery({
    queryKey: ["operations", listParam],
    queryFn: () => getAllOperation(listParam),
  });

  const { data: existingPermissions, isLoading: permissionsLoading } = useQuery({
    queryKey: ["rolePermissions", { ...listParam, roleId }],
    queryFn: () => getAllRolePermission({ ...listParam, categoryId: roleId }),
    enabled: !!roleId,
  });

  const operationsData = useMemo((): OperationItem[] => {
    return (operations?.data || [])
      .map((op) => {
        const row = op as OperationModel;
        return {
          id: row.id || "",
          name: row.name || "",
          moduleId: row.moduleId || "",
          module: row.module?.trim() || "Other",
          subsystemId: row.subsystemId || "",
          subSystem: row.subSystem?.trim() || "Other",
        };
      })
      .sort(
        (a, b) =>
          a.subSystem.localeCompare(b.subSystem) ||
          a.module.localeCompare(b.module) ||
          a.name.localeCompare(b.name),
      );
  }, [operations]);

  // ALL operation ids — the permission map is always built over the full set.
  const operationIds = useMemo(
    () => operationsData.map((op) => op.id).filter(Boolean),
    [operationsData],
  );

  // Cascading Subsystem → Module scope narrows only what is DISPLAYED.
  const visibleOperations = useMemo(() => {
    let rows = operationsData;
    if (scope?.subsystemId) rows = rows.filter((op) => op.subsystemId === scope.subsystemId);
    if (scope?.moduleId) rows = rows.filter((op) => op.moduleId === scope.moduleId);
    return rows;
  }, [operationsData, scope?.subsystemId, scope?.moduleId]);

  const visibleOperationIds = useMemo(
    () => visibleOperations.map((op) => op.id).filter(Boolean),
    [visibleOperations],
  );

  // Reset when the selected role changes.
  useEffect(() => {
    if (loadedRoleRef.current !== null && loadedRoleRef.current !== roleId) {
      loadedRoleRef.current = null;
      setPermissions({});
    }
  }, [roleId]);

  // Initialize ONCE per role from the server rows — belonging to THIS role only.
  useEffect(() => {
    if (!roleId || operationIds.length === 0 || permissionsLoading) return;
    if (loadedRoleRef.current === roleId) return;
    loadedRoleRef.current = roleId;

    const target = roleId.toLowerCase();
    const ownRows = ((existingPermissions?.data ?? []) as unknown as Record<string, unknown>[]).filter(
      (row) => String(row.roleId ?? "").toLowerCase() === target,
    );
    setPermissions(buildPermissionMap(operationIds, ownRows));
  }, [roleId, operationIds, permissionsLoading, existingPermissions]);

  const handlePermissionChange = useCallback(
    (operationId: string, field: PermissionField, checked: boolean) => {
      setPermissions((prev) => setPermissionField(prev, operationId, field, checked));
    },
    [],
  );

  // Check-all only touches the rows the admin can currently SEE.
  const handleCheckAll = useCallback(
    (field: PermissionField, checked: boolean) => {
      if (visibleOperationIds.length === 0) return;
      setPermissions((prev) => setColumnAll(prev, visibleOperationIds, field, checked));
    },
    [visibleOperationIds],
  );

  // Keep the parent's save payload in sync.
  useEffect(() => {
    editHandler(toPermissionDetails(permissions));
  }, [permissions, editHandler]);

  const isLoading = operationsLoading || permissionsLoading;

  const columns: DataTableColumnModel[] = useMemo(
    () => [
      {
        name: "subSystem",
        label: "Subsystem",
        width: "min-w-[110px]",
        render: (text: string) => (
          <span className="text-xs font-medium text-muted">{text}</span>
        ),
      },
      {
        name: "module",
        label: "Module",
        width: "min-w-[150px]",
        render: (text: string) => (
          <span className="text-xs font-medium text-muted">{text}</span>
        ),
      },
      {
        name: "name",
        label: "Operation",
        width: "min-w-[220px]",
        render: (text: string) => (
          <span className="font-medium text-foreground">{text}</span>
        ),
      },
      ...PERMISSION_COLUMNS.map(({ field, label }) => ({
        name: field,
        label: (
          <PermissionColumnHeader
            label={label}
            field={field}
            permissions={permissions}
            operationIds={visibleOperationIds}
            onCheckAll={handleCheckAll}
          />
        ),
        width: "w-28",
        render: (_text: string, record?: OperationItem) => (
          <PermissionCheckbox
            operationId={record?.id || ""}
            field={field}
            permissions={permissions}
            onChange={handlePermissionChange}
            ariaLabel={`${label} permission for ${record?.name ?? "operation"}`}
          />
        ),
      })),
    ],
    [permissions, visibleOperationIds, handleCheckAll, handlePermissionChange],
  );

  return (
    <DataTableProvider
      dataTable={{
        isLoading,
        columns,
        data: visibleOperations as never,
        key: "id",
      }}
    />
  );
}

export default RolePermissionDetail;
