"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Coins, HeartPulse, Info, UserCheck, X } from "lucide-react";
import SearchableSelect from "@/components/common/searchableSelect";
import { confirm } from "@/components/common/dialog";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";
import {
  getPositionEntitlements, savePositionEntitlement, deletePositionEntitlement,
} from "@/services/admin/positionEntitlement";
import { getAllAllowanceTypes, getAllBenefitPlans } from "@/services/admin/compensation";
import { parameterInitialData } from "@/constants/initialization";
import type { PositionEntitlementModel } from "@/models";

const lookupParam = { ...parameterInitialData, take: 20 };

const money = (v?: number | null) =>
  v == null ? "" : Number(v).toLocaleString(undefined, { minimumFractionDigits: 2 });

/**
 * What a POST carries beyond its salary.
 *
 * <p>Lives inside the Position Class form because that is what it belongs to: the class is the job
 * definition, and this says what the job is worth beyond its pay point. It is read when somebody
 * ACTS in the post — a deputy covering it receives these alongside its salary.</p>
 *
 * <p>⚠️ Only rendered for a SAVED class. An entitlement needs a position class to hang off, so
 * offering the panel while creating one would present controls that cannot work — the panel says
 * why instead of appearing disabled for no stated reason.</p>
 */
function PositionEntitlements({ positionClassId }: { positionClassId?: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [busy, setBusy] = useState(false);

  const enabled = !!positionClassId;
  const { data, isLoading } = useQuery({
    queryKey: ["positionEntitlements", positionClassId],
    queryFn: () => getPositionEntitlements(positionClassId!),
    enabled,
  });

  const rows = data ?? [];
  const refresh = () =>
    queryClient.invalidateQueries({ queryKey: ["positionEntitlements", positionClassId] });

  const add = async (m: PositionEntitlementModel) => {
    setBusy(true);
    try {
      await savePositionEntitlement({ ...m, positionClassId });
      toast.success(t("Entitlement added"));
      refresh();
    } catch (e) {
      // The server refuses a duplicate for the same post — surface its wording, not a generic one.
      toast.error(e instanceof Error ? e.message : t("Could not add that entitlement."));
    }
    setBusy(false);
  };

  const patch = async (row: PositionEntitlementModel, change: Partial<PositionEntitlementModel>) => {
    setBusy(true);
    try {
      await savePositionEntitlement({ ...row, ...change, positionClassId });
      refresh();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not save that change."));
    }
    setBusy(false);
  };

  const remove = async (row: PositionEntitlementModel) => {
    if (!row.id) return;
    const ok = await confirm({
      title: t("Remove this entitlement?") ?? "Remove this entitlement?",
      // ⚠️ Worth saying plainly: removing the DEFINITION does not reach back into anybody's pay.
      message:
        t("The post stops carrying it from now on. Allowances already granted to a deputy keep their own dates and are not affected.") ?? "",
      confirmLabel: t("Remove") ?? "Remove",
      variant: "destructive" as const,
    });
    if (!ok) return;
    setBusy(true);
    try {
      await deletePositionEntitlement(row.id);
      toast.success(t("Entitlement removed"));
      refresh();
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not remove that entitlement."));
    }
    setBusy(false);
  };

  if (!enabled) {
    return (
      <div className="mt-4 rounded-lg border border-dashed border-border bg-card p-4">
        <h4 className="flex items-center gap-1.5 text-sm font-semibold text-foreground">
          <UserCheck size={14} className="text-primary" /> {t("What this post carries")}
        </h4>
        <p className="mt-1 text-xs text-muted">
          {t("Save the position class first — allowances and benefits attach to it once it exists.")}
        </p>
      </div>
    );
  }

  return (
    <div className="mt-4 rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex flex-wrap items-start justify-between gap-2">
        <div className="min-w-0">
          <h4 className="flex items-center gap-1.5 text-sm font-semibold text-foreground">
            <UserCheck size={14} className="text-primary" /> {t("What this post carries")}
          </h4>
          <p className="text-xs text-muted">
            {t("Allowances and benefits that belong to the POST, not to whoever holds it. Somebody acting in this post receives them alongside its salary.")}
          </p>
        </div>
        <div className="flex w-full flex-col gap-2 sm:w-auto sm:flex-row sm:items-center">
          <SearchableSelect
            className="sm:w-52"
            queryKey="entitlementAllowanceOptions"
            placeholder={t("Add an allowance…") ?? "Add an allowance…"}
            disabled={busy}
            excludeIds={rows.filter((r) => r.allowanceTypeId).map((r) => r.allowanceTypeId!)}
            fetchOptions={async (term) => {
              const res = await getAllAllowanceTypes({ ...lookupParam, searchText: term });
              return (res?.data ?? [])
                .filter((a) => !!a.id)
                .map((a) => ({
                  id: a.id!,
                  label: a.name ?? "",
                  hint: a.calcMethod === "PercentOfBase" ? `${a.defaultRate ?? 0}%` : money(a.defaultRate),
                }));
            }}
            onSelect={(o) => add({ kind: "Allowance", allowanceTypeId: o.id, grantedWhenActing: true, isActive: true })}
          />
          <SearchableSelect
            className="sm:w-52"
            queryKey="entitlementBenefitOptions"
            placeholder={t("Add a benefit plan…") ?? "Add a benefit plan…"}
            disabled={busy}
            excludeIds={rows.filter((r) => r.benefitPlanId).map((r) => r.benefitPlanId!)}
            fetchOptions={async (term) => {
              const res = await getAllBenefitPlans({ ...lookupParam, searchText: term });
              return (res?.data ?? [])
                .filter((b) => !!b.id)
                .map((b) => ({ id: b.id!, label: b.name ?? "", hint: b.category }));
            }}
            onSelect={(o) => add({ kind: "BenefitPlan", benefitPlanId: o.id, grantedWhenActing: true, isActive: true })}
          />
        </div>
      </div>

      {isLoading && <Loading />}

      {!isLoading && rows.length === 0 && (
        <p className="rounded-md border border-dashed border-border px-3 py-4 text-center text-xs text-muted">
          {t("This post carries nothing beyond its salary. A deputy acting in it would receive the salary only.")}
        </p>
      )}

      {rows.length > 0 && (
        <div className="divide-y divide-border rounded-md border border-border">
          {rows.map((r) => (
            <div key={r.id} className="flex flex-wrap items-center gap-2 px-3 py-2">
              <span className="flex shrink-0 items-center gap-1.5 text-sm text-foreground">
                {r.kind === "BenefitPlan"
                  ? <HeartPulse size={13} className="text-info" />
                  : <Coins size={13} className="text-primary" />}
                <span className="font-medium">{r.referenceName ?? "—"}</span>
              </span>

              {/* The post's own figure, or the catalogue's. Blank means "use the default", which is
                  why the placeholder shows what that default actually is rather than "0". */}
              {r.kind === "Allowance" && (
                <span className="flex items-center gap-1 text-xs text-muted">
                  <input
                    type="number"
                    className="w-28 rounded-md border border-border bg-card px-2 py-1 text-xs text-foreground focus:border-primary focus:outline-none"
                    defaultValue={r.value ?? ""}
                    placeholder={
                      r.defaultRate != null
                        ? `${t("default")} ${r.calcMethod === "PercentOfBase" ? `${r.defaultRate}%` : money(r.defaultRate)}`
                        : t("no default") ?? ""
                    }
                    disabled={busy}
                    onBlur={(e) => {
                      const next = e.target.value === "" ? null : Number(e.target.value);
                      if (next !== (r.value ?? null)) patch(r, { value: next });
                    }}
                  />
                  {r.calcMethod === "PercentOfBase" && <span>{t("% of base")}</span>}
                </span>
              )}

              <label className="ml-auto flex shrink-0 cursor-pointer items-center gap-1.5 text-xs text-muted">
                <input
                  type="checkbox"
                  className="h-3.5 w-3.5 accent-[var(--primary)]"
                  checked={r.grantedWhenActing !== false}
                  disabled={busy}
                  onChange={(e) => patch(r, { grantedWhenActing: e.target.checked })}
                />
                {t("Goes to a deputy")}
              </label>

              <button
                type="button"
                onClick={() => remove(r)}
                disabled={busy}
                // A filled hover rather than a colour shift — the icon is already text-error, so
                // hover:text-error would change nothing visible. Both are registered in theme.css;
                // `palette/no-unregistered-utility` now fails the lint if a variant is not.
                className="shrink-0 rounded p-1 text-error hover:bg-error/10"
                aria-label={t("Remove") ?? "Remove"}
              >
                <X size={13} />
              </button>
            </div>
          ))}
        </div>
      )}

      {/* ⚠️ The one thing somebody will otherwise assume. */}
      <p className="mt-2 flex items-start gap-1.5 rounded-md border border-info/20 bg-info/15 px-2.5 py-1.5 text-[11px] text-muted">
        <Info size={12} className="mt-0.5 shrink-0" />
        {t("Adding an entitlement does not give it to whoever currently holds this post — it describes the job. It is applied to a deputy when they act in it, and withdrawn when the cover ends.")}
      </p>
    </div>
  );
}

export default memo(PositionEntitlements);
