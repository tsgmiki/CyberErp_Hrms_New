"use client";

import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Ban } from "lucide-react";
import { getAllApprovalDelegations, revokeDelegation } from "@/services/admin/approvalDelegation";
import { workflowEntityTypeLabel } from "@/constants/orgStructure";
import { confirm } from "@/components/common/dialog";
import { toast } from "@/components/common/toast";
import type { ApprovalDelegationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/** Status pill. The four states are derived server-side from the dates, never stored. */
const STATUS_TONE: Record<string, string> = {
  Active: "bg-success/15 text-success",
  Scheduled: "bg-info/15 text-info",
  Revoked: "bg-error/15 text-error",
  Expired: "bg-muted/30 text-muted",
};

const day = (v?: string | null) => (v ? String(v).slice(0, 10) : "—");

function ApprovalDelegationList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const list = useEntityList({
    queryKey: "approvalDelegations",
    fetchPage: getAllApprovalDelegations,
  });

  const revoke = async (record: ApprovalDelegationModel) => {
    if (!record.id) return;
    // ⚠️ Withdrawal is terminal and deliberately not undoable, so it asks first.
    const ok = await confirm({
      title: t("Withdraw this delegation?") ?? "Withdraw this delegation?",
      message:
        t("The stand-in stops being able to act immediately, whatever the end date says. This cannot be undone.") ??
        "",
      confirmLabel: t("Withdraw") ?? "Withdraw",
      // Terminal and not undoable — it reads as the destructive action it is.
      variant: "destructive" as const,
    });
    if (!ok) return;

    try {
      await revokeDelegation(record.id);
      toast.success(t("Delegation withdrawn"));
      queryClient.invalidateQueries({ queryKey: ["approvalDelegations"] });
    } catch (e) {
      toast.error(e instanceof Error ? e.message : t("Could not withdraw that delegation."));
    }
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "fromEmployeeName",
          label: "Approver",
          render: (text: string, record: ApprovalDelegationModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text || "—"}
            </button>
          ),
        },
        { name: "toEmployeeName", label: "Stand-in", render: (v: string) => v || "—" },
        {
          name: "startDate",
          label: "Period",
          render: (_v: unknown, r: ApprovalDelegationModel) => (
            <span className="tabular-nums">{day(r.startDate)} → {day(r.endDate)}</span>
          ),
        },
        {
          name: "allProcesses",
          label: "Covers",
          render: (_v: unknown, r: ApprovalDelegationModel) =>
            r.allProcesses ? (
              <span className="text-xs font-medium">{t("All processes")}</span>
            ) : (
              <span className="text-xs">
                {(r.entityTypes ?? []).map(workflowEntityTypeLabel).join(", ") || "—"}
              </span>
            ),
        },
        {
          name: "approvalLimit",
          label: "Ceiling",
          render: (v: unknown) =>
            v === null || v === undefined ? (
              <span className="text-xs italic text-muted">{t("No limit")}</span>
            ) : (
              <span className="tabular-nums">{Number(v).toLocaleString()}</span>
            ),
        },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded px-1.5 py-0.5 text-[11px] font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>
              {t(v)}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: ApprovalDelegationModel) =>
            // Only a live delegation can be withdrawn; an expired or already-revoked one has
            // nothing left to withdraw, and offering the button would be a dead control.
            record.status === "Active" || record.status === "Scheduled" ? (
              <button
                type="button"
                onClick={() => revoke(record)}
                className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs text-error hover:bg-error/10"
              >
                <Ban size={12} /> {t("Withdraw")}
              </button>
            ) : (
              <span className="text-[11px] italic text-muted">{t("Closed")}</span>
            ),
        },
      ] as DataTableColumnModel[],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [editHandler, t],
  );

  return (
    <EntityListShell
      listKey="approvalDelegations"
      listLabel="Delegations"
      columns={columns}
      {...list}
    />
  );
}

export default ApprovalDelegationList;
