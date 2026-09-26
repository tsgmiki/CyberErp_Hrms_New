"use client";

import { memo } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { ArrowRight, Ban, ShieldCheck, UserCheck } from "lucide-react";
import { getMyDelegations, revokeDelegation } from "@/services/admin/approvalDelegation";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import { confirm } from "@/components/common/dialog";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";
import { UserData } from "@/store/user";
import type { ApprovalDelegationModel } from "@/models";

const day = (v?: string | null) => (v ? String(v).slice(0, 10) : "—");

const STATUS_TONE: Record<string, string> = {
  Active: "bg-success/15 text-success",
  Scheduled: "bg-info/15 text-info",
  Revoked: "bg-error/15 text-error",
  Expired: "bg-muted/30 text-muted",
};

/**
 * "My Delegations" — the approver's own view, in BOTH directions.
 *
 * <p>⚠️ Two lists, not one. "Authority I lent out" and "authority I am holding" are different
 * questions with different consequences: the first is something you can withdraw, the second is
 * something you are answerable for. A single merged list makes the reader work out which side of
 * each row they are on, and the row that matters most — somebody else's approvals landing in your
 * inbox — is the one that would be easiest to miss.</p>
 */
function MyDelegations() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const me = (UserData.peek() as { employeeId?: string })?.employeeId;

  const { data, isLoading } = useQuery({
    queryKey: ["myDelegations"],
    queryFn: getMyDelegations,
  });

  const rows = data ?? [];
  // Split on the delegator. When the employee link is unknown we cannot tell the two apart, so
  // everything falls into "granted to me" rather than silently claiming authorship of a record.
  const granted = rows.filter((r) => me && r.fromEmployeeId === me);
  const held = rows.filter((r) => !me || r.fromEmployeeId !== me);

  const withdraw = async (record: ApprovalDelegationModel) => {
    if (!record.id) return;
    const ok = await confirm({
      title: t("Withdraw this delegation?") ?? "Withdraw this delegation?",
      message:
        t("Your stand-in stops being able to act immediately, whatever the end date says. This cannot be undone.") ?? "",
      confirmLabel: t("Withdraw") ?? "Withdraw",
      // Terminal and not undoable — it reads as the destructive action it is.
      variant: "destructive" as const,
    });
    if (!ok) return;
    try {
      await revokeDelegation(record.id);
      toast.success(t("Delegation withdrawn"));
      queryClient.invalidateQueries({ queryKey: ["myDelegations"] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not withdraw that delegation."));
    }
  };

  const Card = ({ r, mine }: { r: ApprovalDelegationModel; mine: boolean }) => (
    <div className="rounded-lg border border-border bg-card p-3">
      <div className="flex flex-wrap items-center gap-2">
        <span className="text-sm font-semibold text-foreground">
          {mine ? r.toEmployeeName : r.fromEmployeeName}
        </span>
        <span className={`rounded px-1.5 py-0.5 text-[11px] font-semibold ${STATUS_TONE[r.status ?? ""] ?? "bg-muted/30 text-muted"}`}>
          {t(r.status ?? "")}
        </span>
        <span className="ml-auto text-xs tabular-nums text-muted">
          {day(r.startDate)} <ArrowRight size={11} className="inline" /> {day(r.endDate)}
        </span>
      </div>

      <p className="mt-1 text-xs text-muted">
        {r.allProcesses
          ? t("All processes")
          : (r.entityTypes ?? []).map(workflowEntityTypeLabel).join(", ") || "—"}
        {r.approvalLimit != null && (
          <> · {t("up to")} <span className="tabular-nums">{Number(r.approvalLimit).toLocaleString()}</span></>
        )}
      </p>

      {r.reason && <p className="mt-1 text-xs italic text-muted">“{r.reason}”</p>}

      {mine && (r.status === "Active" || r.status === "Scheduled") && (
        <button
          type="button"
          onClick={() => withdraw(r)}
          className="mt-2 inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs text-error hover:bg-error/10"
        >
          <Ban size={12} /> {t("Withdraw")}
        </button>
      )}
    </div>
  );

  return (
    <div className="flex h-full min-h-0 flex-col gap-4 p-1">
      <header className="flex items-center gap-2">
        <UserCheck className="h-5 w-5 shrink-0 text-primary" />
        <div>
          <h1 className="text-base font-semibold text-foreground">{t("My Delegations")}</h1>
          <p className="text-xs text-muted">
            {t("Approval authority you have lent out, and authority you are currently holding for somebody else.")}
          </p>
        </div>
      </header>

      {isLoading && <Loading />}

      <section>
        <h2 className="mb-2 text-[11px] font-semibold uppercase tracking-wide text-muted">
          {t("Authority I lent out")}
        </h2>
        {granted.length === 0 ? (
          <p className="rounded-lg border border-dashed border-border p-4 text-center text-xs text-muted">
            {t("You have not delegated your approval authority to anyone.")}
          </p>
        ) : (
          <div className="grid gap-2 md:grid-cols-2">
            {granted.map((r) => <Card key={r.id} r={r} mine />)}
          </div>
        )}
      </section>

      <section>
        <h2 className="mb-2 flex items-center gap-1.5 text-[11px] font-semibold uppercase tracking-wide text-muted">
          <ShieldCheck size={12} /> {t("Authority I am holding")}
        </h2>
        {held.length === 0 ? (
          <p className="rounded-lg border border-dashed border-border p-4 text-center text-xs text-muted">
            {t("You are not standing in for anyone.")}
          </p>
        ) : (
          <>
            {/* ⚠️ Said out loud. Approvals arriving in your inbox on somebody else's authority are
                the single most surprising thing this feature does, and the person acting is the one
                best placed to notice if a delegation is wider than they expected. */}
            <p className="mb-2 rounded-md border border-warning/30 bg-warning/10 px-2.5 py-1.5 text-[11px] text-warning">
              {t("Requests covered by these will appear in your approval inbox, marked as acting on their behalf.")}
            </p>
            <div className="grid gap-2 md:grid-cols-2">
              {held.map((r) => <Card key={r.id} r={r} mine={false} />)}
            </div>
          </>
        )}
      </section>
    </div>
  );
}

export default memo(MyDelegations);
