import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";
import type { PermissionState } from "@/components/admin/rolePermission/rolePermissionUtils";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export interface SaveRolePermissionsResult {
  status: "success" | "error";
  message: string;
}

/**
 * Bulk-upserts one role's permission matrix — a direct JSON call to the backend contract
 * (POST /RolePermission { roleId, items[] }). Deliberately NOT FormData/Zod based: the matrix is
 * plain structured state, so nothing is serialized through hidden form fields.
 */
export default async function saveRolePermissions(
  roleId: string,
  details: PermissionState[],
): Promise<SaveRolePermissionsResult> {
  if (!roleId) return { status: "error", message: "Select a role first." };

  try {
    const response = await fetch(`${API_BASE_URL}/RolePermission`, {
      method: "POST",
      credentials: "include",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        roleId,
        items: details.map((d) => ({
          roleId,
          operationId: d.operationId,
          canView: d.canView === "true",
          canAdd: d.canAdd === "true",
          canEdit: d.canEdit === "true",
          canDelete: d.canDelete === "true",
          canApprove: d.canApprove === "true",
        })),
      }),
    });

    if (!response.ok) {
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      return {
        status: "error",
        message: errorMessageParser(parsed.errors || parsed) || "Saving failed",
      };
    }

    return { status: "success", message: "Permissions saved" };
  } catch {
    return { status: "error", message: "Network error" };
  }
}
