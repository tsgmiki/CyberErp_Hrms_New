export type PermissionField =
  | "canView"
  | "canAdd"
  | "canEdit"
  | "canDelete"
  | "canApprove";

export interface PermissionRow {
  canView: boolean;
  canAdd: boolean;
  canEdit: boolean;
  canDelete: boolean;
  canApprove: boolean;
}

/** Permissions keyed by operationId — a stable map (no array reordering on toggle). */
export type PermissionMap = Record<string, PermissionRow>;

/** Kept for the save payload contract (operationId + string flags) that the backend expects. */
export interface PermissionState {
  operationId: string;
  canView: string;
  canAdd: string;
  canEdit: string;
  canDelete: string;
  canApprove: string;
}

export const PERMISSION_FIELDS: PermissionField[] = [
  "canView",
  "canAdd",
  "canEdit",
  "canDelete",
  "canApprove",
];

const EMPTY_ROW: PermissionRow = {
  canView: false,
  canAdd: false,
  canEdit: false,
  canDelete: false,
  canApprove: false,
};

function toBool(value: unknown): boolean {
  if (value === true || value === 1) return true;
  if (typeof value === "string") {
    const normalized = value.trim().toLowerCase();
    return normalized === "true" || normalized === "1" || normalized === "yes";
  }
  return false;
}

/**
 * Build the map for ALL operations, seeding each from the role's existing permission row.
 * Operations without a saved row default to all-false (a new role starts fully unchecked).
 */
export function buildPermissionMap(
  operationIds: string[],
  apiRows: Record<string, unknown>[] | undefined,
): PermissionMap {
  const byOperation = new Map(
    (apiRows ?? []).map((row) => [String(row.operationId ?? ""), row]),
  );
  const map: PermissionMap = {};
  for (const operationId of operationIds) {
    const row = byOperation.get(operationId);
    map[operationId] = row
      ? {
          canView: toBool(row.canView),
          canAdd: toBool(row.canAdd),
          canEdit: toBool(row.canEdit),
          canDelete: toBool(row.canDelete),
          canApprove: toBool(row.canApprove),
        }
      : { ...EMPTY_ROW };
  }
  return map;
}

export function getPermissionField(
  map: PermissionMap,
  operationId: string,
  field: PermissionField,
): boolean {
  return map[operationId]?.[field] ?? false;
}

export function setPermissionField(
  map: PermissionMap,
  operationId: string,
  field: PermissionField,
  checked: boolean,
): PermissionMap {
  return {
    ...map,
    [operationId]: { ...(map[operationId] ?? EMPTY_ROW), [field]: checked },
  };
}

export function setColumnAll(
  map: PermissionMap,
  operationIds: string[],
  field: PermissionField,
  checked: boolean,
): PermissionMap {
  const next: PermissionMap = { ...map };
  for (const operationId of operationIds) {
    next[operationId] = { ...(next[operationId] ?? EMPTY_ROW), [field]: checked };
  }
  return next;
}

export function getColumnCheckState(
  map: PermissionMap,
  operationIds: string[],
  field: PermissionField,
): { checked: boolean; indeterminate: boolean } {
  if (operationIds.length === 0) return { checked: false, indeterminate: false };
  let selected = 0;
  for (const operationId of operationIds) {
    if (getPermissionField(map, operationId, field)) selected += 1;
  }
  return {
    checked: selected === operationIds.length,
    indeterminate: selected > 0 && selected < operationIds.length,
  };
}

/** Serialize the map to the array of string-flag rows the save service + backend expect. */
export function toPermissionDetails(map: PermissionMap): PermissionState[] {
  return Object.entries(map).map(([operationId, row]) => ({
    operationId,
    canView: row.canView ? "true" : "false",
    canAdd: row.canAdd ? "true" : "false",
    canEdit: row.canEdit ? "true" : "false",
    canDelete: row.canDelete ? "true" : "false",
    canApprove: row.canApprove ? "true" : "false",
  }));
}
