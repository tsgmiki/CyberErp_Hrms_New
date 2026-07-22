"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { CheckCircle2, XCircle, Pencil, Ban, Trash2 } from "lucide-react";
import { getAllTrainingNeeds, deleteTrainingNeed, approveTrainingNeed, rejectTrainingNeed, cancelTrainingNeed } from "@/services/admin/trainingNeed";
import type { TrainingNeedModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Approved: "bg-info/15 text-info",
  Fulfilled: "bg-success/15 text-success",
  Rejected: "bg-muted/30 text-muted",
  Cancelled: "bg-muted/30 text-muted",
};

const PRIORITY_TONE: Record<string, string> = {
  Critical: "text-error",
  High: "text-warning",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function TrainingNeedList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [actionMsg, setActionMsg] = useState<string>("");

  // The paged endpoint is role-scoped server-side (HR all / manager subtree+raised / employee own).
  const list = useEntityList({
    queryKey: "trainingNeeds",
    fetchPage: getAllTrainingNeeds,
    deleteById: deleteTrainingNeed,
  });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setActionMsg(res.message);
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["trainingNeeds"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: TrainingNeedModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.employeeNumber}</span>
            </button>
          ),
        },
        {
          name: "topic",
          label: "Training",
          render: (text: string, record: TrainingNeedModel) => (
            <span>
              <span className="block">{record.courseName || text}</span>
              <span className="block text-xs text-muted">
                {record.needType}
                {record.source && record.source !== "Manual" ? ` · ${record.source}` : ""}
              </span>
            </span>
          ),
        },
        {
          name: "priority",
          label: "Priority",
          render: (v: string) => <span className={`text-xs font-semibold ${PRIORITY_TONE[v] ?? "text-muted"}`}>{v}</span>,
        },
        { name: "estimatedCost", label: "Est. Cost", render: (v: number) => (v != null ? Number(v).toLocaleString() : "—") },
        { name: "neededBy", label: "Needed By", render: fmtDate },
        { name: "requestedByName", label: "Requested By", render: (v: string) => v || "—" },
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
          render: (_t: unknown, record: TrainingNeedModel) => {
            const pending = record.status === "Pending";
            const cancellable = record.status === "Pending" || record.status === "Approved";
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
                    onClick={() => record.id && runAction(() => approveTrainingNeed(record.id!))}
                    className="rounded p-1 text-muted hover:text-success"
                  >
                    <CheckCircle2 size={15} />
                  </button>
                )}
                {pending && (
                  <button
                    type="button"
                    title={t("Reject (no-workflow mode; otherwise decide from My Approvals)")}
                    onClick={() => record.id && runAction(() => rejectTrainingNeed(record.id!))}
                    className="rounded p-1 text-muted hover:text-error"
                  >
                    <XCircle size={15} />
                  </button>
                )}
                {cancellable && (
                  <button
                    type="button"
                    title={t("Cancel request")}
                    onClick={() => record.id && runAction(() => cancelTrainingNeed(record.id!))}
                    className="rounded p-1 text-muted hover:text-error"
                  >
                    <Ban size={15} />
                  </button>
                )}
                {pending && (
                  <button type="button" title={t("Delete")} onClick={() => record.id && list.deleteRecord(record.id)} className="rounded p-1 text-muted hover:text-error">
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
      <EntityListShell listKey="trainingNeeds" listLabel="Training Needs" columns={columns} {...list} />
    </div>
  );
}

export default TrainingNeedList;
