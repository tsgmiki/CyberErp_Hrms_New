export type PermissionField =
  | "canView"
  | "canAdd"
  | "canEdit"
  | "canDelete"
  | "canApprove";

export interface PermissionState {
  operationId: string;
  canView: string;
  canAdd: string;
  canEdit: string;
  canDelete: string;
  canApprove: string;
}

const PERMISSION_FIELDS: PermissionField[] = [
  "canView",
  "canAdd",
  "canEdit",
  "canDelete",
  "canApprove",
];

export function permissionValueToBool(value: unknown): boolean {
  if (value === true || value === 1) return true;
  if (typeof value === "string") {
    const normalized = value.trim().toLowerCase();
    return normalized === "true" || normalized === "1" || normalized === "yes";
  }
  return false;
}

export function boolToPermissionValue(checked: boolean): string {
  return checked ? "true" : "false";
}

export function mapApiPermission(record: Record<string, unknown>): PermissionState {
  const operationId = String(record.operationId ?? "");
  const state = {} as PermissionState;
  state.operationId = operationId;
  for (const field of PERMISSION_FIELDS) {
    state[field] = boolToPermissionValue(permissionValueToBool(record[field]));
  }
  return state;
}

export function defaultPermission(operationId: string): PermissionState {
  return {
    operationId,
    canView: "false",
    canAdd: "false",
    canEdit: "false",
    canDelete: "false",
    canApprove: "false",
  };
}

export function getPermissionField(
  permissions: PermissionState[],
  operationId: string,
  field: PermissionField,
): boolean {
  const row = permissions.find((p) => p.operationId === operationId);
  return permissionValueToBool(row?.[field]);
}

export function getColumnCheckState(
  permissions: PermissionState[],
  operationIds: string[],
  field: PermissionField,
): { checked: boolean; indeterminate: boolean } {
  if (operationIds.length === 0) {
    return { checked: false, indeterminate: false };
  }

  let selected = 0;
  for (const id of operationIds) {
    if (getPermissionField(permissions, id, field)) selected += 1;
  }

  return {
    checked: selected === operationIds.length,
    indeterminate: selected > 0 && selected < operationIds.length,
  };
}

export function applyPermissionField(
  permissions: PermissionState[],
  operationId: string,
  field: PermissionField,
  checked: boolean,
): PermissionState[] {
  const val = boolToPermissionValue(checked);
  const existing = permissions.find((p) => p.operationId === operationId);
  const updated: PermissionState = {
    operationId,
    canView: field === "canView" ? val : (existing?.canView ?? "false"),
    canAdd: field === "canAdd" ? val : (existing?.canAdd ?? "false"),
    canEdit: field === "canEdit" ? val : (existing?.canEdit ?? "false"),
    canDelete: field === "canDelete" ? val : (existing?.canDelete ?? "false"),
    canApprove: field === "canApprove" ? val : (existing?.canApprove ?? "false"),
  };
  return [...permissions.filter((p) => p.operationId !== operationId), updated];
}

export function applyCheckAll(
  permissions: PermissionState[],
  operationIds: string[],
  field: PermissionField,
  checked: boolean,
): PermissionState[] {
  const val = boolToPermissionValue(checked);
  const byId = new Map(permissions.map((p) => [p.operationId, { ...p }]));

  for (const operationId of operationIds) {
    const row = byId.get(operationId) ?? defaultPermission(operationId);
    row[field] = val;
    byId.set(operationId, row);
  }

  return operationIds.map((id) => byId.get(id) ?? defaultPermission(id));
}

export function buildPermissionsFromOperations(
  operationIds: string[],
  apiRows: Record<string, unknown>[] | undefined,
): PermissionState[] {
  const apiByOp = new Map(
    (apiRows ?? []).map((row) => [String(row.operationId ?? ""), row]),
  );

  return operationIds.map((operationId) => {
    const api = apiByOp.get(operationId);
    return api ? mapApiPermission(api) : defaultPermission(operationId);
  });
}
