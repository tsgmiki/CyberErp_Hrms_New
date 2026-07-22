import { DataTableCheckbox } from "@/components/common/dataTableProvider/dataTableCheckbox";
import type { PermissionField, PermissionMap } from "./rolePermissionUtils";
import { getColumnCheckState, getPermissionField } from "./rolePermissionUtils";

interface PermissionCheckboxProps {
  operationId: string;
  field: PermissionField;
  permissions: PermissionMap;
  onChange: (operationId: string, field: PermissionField, checked: boolean) => void;
  ariaLabel: string;
}

export function PermissionCheckbox({
  operationId,
  field,
  permissions,
  onChange,
  ariaLabel,
}: PermissionCheckboxProps) {
  return (
    <div className="flex justify-center py-0.5">
      <DataTableCheckbox
        checked={getPermissionField(permissions, operationId, field)}
        onChange={(checked) => onChange(operationId, field, checked)}
        ariaLabel={ariaLabel}
      />
    </div>
  );
}

interface PermissionColumnHeaderProps {
  label: string;
  field: PermissionField;
  permissions: PermissionMap;
  operationIds: string[];
  onCheckAll: (field: PermissionField, checked: boolean) => void;
}

export function PermissionColumnHeader({
  label,
  field,
  permissions,
  operationIds,
  onCheckAll,
}: PermissionColumnHeaderProps) {
  const { checked, indeterminate } = getColumnCheckState(
    permissions,
    operationIds,
    field,
  );

  return (
    <div className="flex flex-col items-center justify-center gap-1.5">
      <DataTableCheckbox
        checked={checked}
        indeterminate={indeterminate}
        onChange={(value) => onCheckAll(field, value)}
        ariaLabel={`Select all ${label}`}
      />
      <span className="text-[10px] font-semibold uppercase tracking-wider text-foreground">
        {label}
      </span>
    </div>
  );
}
