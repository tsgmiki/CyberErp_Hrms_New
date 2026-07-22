"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Play, Ban, Pencil, FileText, Trash2, MoveRight } from "lucide-react";
import { getAllTransferRequests } from "@/services/admin/transferRequest";
import { deleteMovement, executeMovement, cancelMovement } from "@/services/admin/employee/personnelActions";
import type { EmployeeMovementModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import NoticeModal from "./noticeModal";

interface Props {
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Approved: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function TransferRequestList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [noticeFor, setNoticeFor] = useState<EmployeeMovementModel | null>(null);
  const [actionMsg, setActionMsg] = useState<string>("");

  // The screen is transfer-centric; the paged endpoint is role-scoped server-side.
  const list = useEntityList({
    queryKey: "transferRequests",
    fetchPage: getAllTransferRequests,
    deleteById: deleteMovement,
    initialParam: { movementType: "Transfer" },
  });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setActionMsg(res.message);
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["transferRequests"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: EmployeeMovementModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text || "—"}</span>
              <span className="block text-xs text-muted">{record.employeeNumber}</span>
            </button>
          ),
        },
        {
          name: "transferKind",
          label: "Kind",
          render: (text: string) =>
            text ? <span className="rounded bg-primary/10 px-2 py-0.5 text-xs font-semibold text-primary">{text}</span> : "—",
        },
        {
          name: "toPositionName",
          label: "Change",
          render: (_t: unknown, r: EmployeeMovementModel) => (
            <span className="inline-flex items-center gap-1.5 text-xs">
              <span className="text-muted">{r.fromPositionName || "—"}</span>
              <MoveRight size={12} className="shrink-0 text-muted" />
              <span className="font-medium">{r.toPositionName || "—"}</span>
            </span>
          ),
        },
        { name: "effectiveDate", label: "Effective Date", sort: true, render: fmtDate },
        { name: "requestedByName", label: "Requested By", render: (v: string) => v || "—" },
        {
          name: "status",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? "bg-muted/30 text-muted"}`}>
              {text}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: EmployeeMovementModel) => {
            const pending = r.status === "Pending";
            const executable = r.status === "Pending" || r.status === "Approved";
            return (
              <span className="inline-flex items-center gap-0.5">
                <button type="button" title={t("Edit") ?? ""} disabled={!pending}
                  onClick={() => r.id && editHandler(r.id)}
                  className="rounded p-1 text-primary hover:bg-primary/10 disabled:cursor-not-allowed disabled:opacity-40">
                  <Pencil size={15} />
                </button>
                <button type="button" title={t("Execute now") ?? ""} disabled={!executable}
                  onClick={() => r.id && runAction(() => executeMovement(r.id!))}
                  className="rounded p-1 text-success hover:bg-success/10 disabled:cursor-not-allowed disabled:opacity-40">
                  <Play size={15} />
                </button>
                <button type="button" title={t("Cancel") ?? ""} disabled={!executable}
                  onClick={() => r.id && runAction(() => cancelMovement(r.id!))}
                  className="rounded p-1 text-warning hover:bg-warning/10 disabled:cursor-not-allowed disabled:opacity-40">
                  <Ban size={15} />
                </button>
                <button type="button" title={t("Transfer notice") ?? ""}
                  onClick={() => setNoticeFor(r)}
                  className="rounded p-1 text-foreground hover:bg-secondary/40">
                  <FileText size={15} />
                </button>
                <button type="button" title={t("Delete") ?? ""} disabled={r.status === "Completed"}
                  onClick={() => r.id && list.deleteRecord(r.id)}
                  className="rounded p-1 text-error hover:bg-error/10 disabled:cursor-not-allowed disabled:opacity-40">
                  <Trash2 size={15} />
                </button>
              </span>
            );
          },
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <>
      {actionMsg && <p className="px-3 pt-2 text-xs text-muted">{actionMsg}</p>}
      <EntityListShell listKey="transferRequests" listLabel="Transfer Requests" columns={columns} {...list} />
      {noticeFor?.id && <NoticeModal movement={noticeFor} onClose={() => setNoticeFor(null)} />}
    </>
  );
}

export default TransferRequestList;
