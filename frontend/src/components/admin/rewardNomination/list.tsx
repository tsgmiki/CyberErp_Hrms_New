"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Pencil, Trash2 } from "lucide-react";
import { getAllNominations, deleteNomination, approveNomination, rejectNomination } from "@/services/admin/rewardNomination";
import type { RewardNominationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-muted/30 text-muted",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function RewardNominationList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [actionMsg, setActionMsg] = useState<string>("");

  // The paged endpoint is role-scoped server-side (HR all / manager subtree+raised / employee own).
  const list = useEntityList({
    queryKey: "rewardNominations",
    fetchPage: getAllNominations,
    deleteById: deleteNomination,
  });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setActionMsg(res.message);
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["rewardNominations"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "nomineeName",
          label: "Nominee",
          sort: true,
          render: (text: string, record: RewardNominationModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.nomineeNumber}</span>
            </button>
          ),
        },
        {
          name: "badgeName",
          label: "Award",
          render: (text: string, record: RewardNominationModel) => (
            <span className="flex items-center gap-2">
              <span className="inline-block h-3 w-3 rounded-full" style={{ backgroundColor: record.badgeColor || "var(--color-primary, #888)" }} />
              <span>
                <span className="block">{text || "—"}</span>
                <span className="block text-xs text-muted">
                  {record.rewardKind}
                  {record.pointsValue ? ` · ${record.pointsValue} pts` : ""}
                  {record.monetaryValue ? ` · ${record.monetaryValue}` : ""}
                </span>
              </span>
            </span>
          ),
        },
        { name: "programName", label: "Program", render: (v: string) => v || "—" },
        { name: "nominatedByName", label: "Nominated By", render: (v: string) => v || "—" },
        { name: "nominatedOn", label: "Date", sort: true, render: fmtDate },
        {
          name: "status",
          label: "Status",
          render: (v: string) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-secondary/40 text-foreground"}`}>{v}</span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: RewardNominationModel) => {
            const pending = record.status === "Pending";
            return (
              <span className="flex items-center gap-1.5">
                {pending && (
                  <button type="button" title={t("Edit")} onClick={() => record.id && editHandler(record.id)} className="rounded p-1 text-muted hover:text-primary">
                    <Pencil size={15} />
                  </button>
                )}
                {pending && (
                  <button
                    type="button"
                    title={t("Approve (no-workflow mode; otherwise decide from My Approvals)")}
                    onClick={() => record.id && runAction(() => approveNomination(record.id!))}
                    className="rounded p-1 text-muted hover:text-success"
                  >
                    <CheckCircle2 size={15} />
                  </button>
                )}
                {pending && (
                  <button
                    type="button"
                    title={t("Reject (no-workflow mode; otherwise decide from My Approvals)")}
                    onClick={() => record.id && runAction(() => rejectNomination(record.id!))}
                    className="rounded p-1 text-muted hover:text-error"
                  >
                    <XCircle size={15} />
                  </button>
                )}
                {pending && (
                  <button type="button" title={t("Withdraw")} onClick={() => record.id && list.deleteRecord(record.id)} className="rounded p-1 text-muted hover:text-error">
                    <Trash2 size={15} />
                  </button>
                )}
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="space-y-2">
      {actionMsg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{actionMsg}</p>}
      <EntityListShell listKey="rewardNominations" listLabel="Award Nominations" columns={columns} {...list} />
    </div>
  );
}

export default RewardNominationList;
