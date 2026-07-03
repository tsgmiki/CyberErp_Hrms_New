"use client";
import DataTableProvider from "../../common/dataTableProvider/dataTableProvider";
import { useQuery } from "@tanstack/react-query";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import type {
  DataTableColumnModel,
  OperationModel,
  ParameterModel,
} from "@/models";
import { parameterInitialData } from "@/constants/initialization";
import getAllModule from "@/services/admin/module/getAll";
import getAllRolePermission from "@/services/admin/rolePermission/getAll";
import getAllOperation from "@/services/admin/operation/getAll";
import {
  PermissionCheckbox,
  PermissionColumnHeader,
} from "./permissionCheckbox";
import {
  applyCheckAll,
  applyPermissionField,
  buildPermissionsFromOperations,
  type PermissionField,
  type PermissionState,
} from "./rolePermissionUtils";

interface RolePermissionDetailProps {
  roleId: string;
  editHandler: (permissions: PermissionState[]) => void;
}

interface OperationItem {
  id: string;
  name: string;
  moduleId: string;
  module?: string;
  subSystem: string;
}

const PERMISSION_COLUMNS: { field: PermissionField; label: string }[] = [
  { field: "canView", label: "View" },
  { field: "canAdd", label: "Add" },
  { field: "canEdit", label: "Edit" },
  { field: "canDelete", label: "Delete" },
  { field: "canApprove", label: "Approve" },
];

function RolePermissionDetail({
  roleId,
  editHandler,
}: RolePermissionDetailProps) {
  const { t } = useTranslation();
  const [permissions, setPermissions] = useState<PermissionState[]>([]);
  const initializedKeyRef = useRef<string | null>(null);

  const [listParam] = useState<ParameterModel>({
    ...parameterInitialData,
    take: 1000,
  });

  const { data: operations, isLoading: operationsLoading } = useQuery({
    queryKey: ["operations", listParam],
    queryFn: () => getAllOperation(listParam),
  });

  const { data: modules, isLoading: modulesLoading } = useQuery({
    queryKey: ["modules", listParam],
    queryFn: () => getAllModule(listParam),
  });

  const { data: existingPermissions, isLoading: permissionsLoading } = useQuery(
    {
      queryKey: ["rolePermissions", { ...listParam, roleId }],
      queryFn: () => getAllRolePermission({ ...listParam, categoryId: roleId }),
      enabled: !!roleId,
    },
  );

  const subSystemByModuleId = useMemo(() => {
    const map = new Map<string, string>();
    for (const module of modules?.data ?? []) {
      if (module.id) {
        map.set(module.id, (module.subSystem || "").trim());
      }
    }
    return map;
  }, [modules?.data]);

  const operationsData = useMemo((): OperationItem[] => {
    return (operations?.data || []).map((op) => {
      const row = op as OperationModel;
      const subSystem =
        row.subSystem?.trim() ||
        subSystemByModuleId.get(row.moduleId || "") ||
        "";

      return {
        id: row.id || "",
        name: row.name || "",
        moduleId: row.moduleId || "",
        module: row.module?.trim() || row.moduleId || "",
        subSystem,
      };
    });
  }, [operations, subSystemByModuleId]);

  const operationIds = useMemo(
    () => operationsData.map((op) => op.id),
    [operationsData],
  );

  useEffect(() => {
    initializedKeyRef.current = null;
    setPermissions([]);
  }, [roleId]);

  useEffect(() => {
    if (!roleId || operationsData.length === 0 || permissionsLoading) return;

    const initKey = `${roleId}:${operationIds.join(",")}:${existingPermissions?.data?.length ?? 0}`;
    if (initializedKeyRef.current === initKey) return;

    setPermissions(
      buildPermissionsFromOperations(
        operationIds,
        existingPermissions?.data as Record<string, unknown>[] | undefined,
      ),
    );
    initializedKeyRef.current = initKey;
  }, [
    roleId,
    operationsData.length,
    operationIds,
    existingPermissions,
    permissionsLoading,
  ]);

  const handlePermissionChange = useCallback(
    (operationId: string, field: PermissionField, checked: boolean) => {
      setPermissions((prev) =>
        applyPermissionField(prev, operationId, field, checked),
      );
    },
    [],
  );

  const handleCheckAll = useCallback(
    (field: PermissionField, checked: boolean) => {
      if (operationIds.length === 0) return;
      setPermissions((prev) =>
        applyCheckAll(prev, operationIds, field, checked),
      );
    },
    [operationIds],
  );

  useEffect(() => {
    editHandler(permissions);
  }, [permissions, editHandler]);

  const isLoading = operationsLoading || modulesLoading || permissionsLoading;

  const getGroupLabel = useCallback(
    (key: string, rows: Record<string, unknown>[]) => {
      const subsystemName =
        key?.trim() || t("Unassigned subsystem", "Unassigned subsystem");
      const countLabel =
        rows.length === 1
          ? t("1 operation", "1 operation")
          : t("{{count}} operations", {
              count: rows.length,
              defaultValue: `${rows.length} operations`,
            });
      return `${subsystemName} · ${countLabel}`;
    },
    [t],
  );

  const columns: DataTableColumnModel[] = useMemo(
    () => [
      {
        name: "name",
        label: "Operation",
        width: "min-w-[200px]",
        render: (text: string, record?: OperationItem) => (
          <div className="min-w-0">
            <span className="font-medium text-foreground">{text}</span>
            {record?.module ? (
              <span className="mt-0.5 block truncate text-xs text-muted">
                {record.module}
              </span>
            ) : null}
          </div>
        ),
      },
      {
        name: "module",
        label: "Module",
      },
      ...PERMISSION_COLUMNS.map(({ field, label }) => ({
        name: field,
        label: (
          <PermissionColumnHeader
            label={label}
            field={field}
            permissions={permissions}
            operationIds={operationIds}
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
    [permissions, operationIds, handleCheckAll, handlePermissionChange],
  );

  const tableKey =
    permissions
      .map(
        (p) =>
          `${p.operationId}-${p.canView}-${p.canAdd}-${p.canEdit}-${p.canDelete}-${p.canApprove}`,
      )
      .join(",") || "empty";

  return (
    <div className="m-2" key={tableKey}>
      <DataTableProvider
        dataTable={{
          isLoading,
          columns,
          data: operationsData as never,
          key: "id",
          groupBy: "subSystem",
          getGroupLabel,
        }}
      />
    </div>
  );
}

export default RolePermissionDetail;
