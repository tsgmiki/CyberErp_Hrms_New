"use client";
import { lazy, memo, useCallback, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ShieldCheck, Save } from "lucide-react";
import { EntityModuleShell } from "@/template";
import DropDownField from "@/components/ui/dropDownField";
import ButtonField from "@/components/ui/buttonField";
import { toast } from "@/components/common/toast";
import { parameterInitialData } from "@/constants/initialization";
import getAllRole from "@/services/admin/role/getAll";
import saveRolePermissions from "@/services/admin/rolePermission/save";
import type { PermissionState } from "./rolePermissionUtils";

const RolePermissionDetail = memo(lazy(() => import("./detail")));

/**
 * Role Permissions — deliberately a SIMPLE, self-contained screen (no form framework):
 * pick a role, tick the matrix, Save. The role selection and the ticked state survive saving,
 * so the admin can keep adjusting without anything resetting or disappearing.
 */
function RolePermission() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [role, setRole] = useState<{ id: string; name: string } | null>(null);
  const [details, setDetails] = useState<PermissionState[]>([]);
  const [saving, setSaving] = useState(false);

  const [roleParam, setRoleParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: roles, isLoading: rolesLoading } = useQuery({
    queryKey: ["roles", roleParam],
    queryFn: () => getAllRole(roleParam),
  });

  const detailHandler = useCallback((next: PermissionState[]) => setDetails(next), []);

  const save = async () => {
    if (!role) return;
    setSaving(true);
    const result = await saveRolePermissions(role.id, details);
    setSaving(false);
    if (result.status === "success") {
      toast.success(t(result.message));
      // Refresh the cached server rows; the matrix keeps the user's current (now saved) state.
      queryClient.invalidateQueries({ queryKey: ["rolePermissions"] });
      queryClient.invalidateQueries({ queryKey: ["moduleWithOperations"] });
    } else {
      toast.error(result.message);
    }
  };

  return (
    <EntityModuleShell
      title="Role Permissions"
      headerDescription="Grant each role its menu operations: View controls visibility, the other flags gate the screen's actions"
      headerIcon={<ShieldCheck className="h-6 w-6 text-primary" />}
      tableTitle="Role Permissions"
      hideAdd
      hideBack
      showForm={false}
      onList={() => undefined}
      onAdd={() => undefined}
    >
      <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
        <div className="flex flex-wrap items-end gap-3">
          <div className="w-80">
            <DropDownField
              type="dropDown"
              name="roleId"
              label="Role"
              placeholder={t("Select a role…")}
              value={role?.id ?? ""}
              displayValue={role?.name ?? ""}
              param={roleParam}
              setParam={setRoleParam as never}
              isLoading={rolesLoading}
              data={(roles?.data ?? []).map((r) => ({ id: r.id, name: r.name })) as never}
              onSelect={(_name: string, item: { id: string; name: string }) =>
                setRole({ id: item.id, name: item.name })
              }
            />
          </div>
          <ButtonField
            value={saving ? "Saving…" : "Save Permissions"}
            variant="primary"
            icon={<Save size={15} />}
            disabled={!role || saving}
            onClick={save}
          />
        </div>

        {role ? (
          <div className="min-h-0 flex-1 overflow-auto">
            <RolePermissionDetail roleId={role.id} editHandler={detailHandler} />
          </div>
        ) : (
          <p className="rounded-lg border border-dashed border-border p-8 text-center text-sm text-muted">
            {t("Select a role to load its permission matrix.")}
          </p>
        )}
      </div>
    </EntityModuleShell>
  );
}

export default RolePermission;
