"use client";

import { useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQueryClient } from "@tanstack/react-query";
import { Play, Ban, Pencil, FileText, Trash2, MoveRight } from "lucide-react";
import { getAllTransferRequests } from "@/services/admin/transferRequest";
import { deleteMovement, executeMovement, cancelMovement } from "@/services/admin/employee/personnelActions";
import Badge, { type BadgeVariant } from "@/components/common/badge/badge";
import type { EmployeeMovementModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import NoticeModal from "./noticeModal";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` is a MovementStatus ENUM NAME, not "true"/"false" —
 * `Enum.TryParse<MovementStatus>(request.Status, true, out …)`. It sits alongside the
 * `movementType: "Transfer"` the list already pins, and the filter patches `param` rather than
 * replacing it, so that pin survives.
 */
const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "Pending", label: "Pending" },
      { value: "Approved", label: "Approved" },
      { value: "Completed", label: "Completed" },
      { value: "Cancelled", label: "Cancelled" },
    ],
  },
];

/** Movement status → the shared Badge's variants, which the hand-rolled tones mapped onto anyway. */
const STATUS_VARIANT: Record<string, BadgeVariant> = {
  Pending: "warning",
  Approved: "info",
  Completed: "success",
  Cancelled: "muted",
};

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : null);

function TransferRequestList({ editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [noticeFor, setNoticeFor] = useState<EmployeeMovementModel | null>(null);
  // ⚠️ The outcome is kept with the message. It used to render as muted grey either way, so a
  // REFUSED execute — "this movement cannot be executed yet" — looked exactly like a successful
  // one, on a screen whose buttons change somebody's position and salary.
  const [action, setAction] = useState<{ ok: boolean; message: string } | null>(null);

  // The screen is transfer-centric; the paged endpoint is role-scoped server-side.
  const list = useEntityList({
    queryKey: "transferRequests",
    fetchPage: getAllTransferRequests,
    deleteById: deleteMovement,
    initialParam: { movementType: "Transfer" },
  });

  const runAction = async (fn: () => Promise<{ ok: boolean; message: string }>) => {
    const res = await fn();
    setAction({ ok: res.ok, message: res.message });
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["transferRequests"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: EmployeeMovementModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold text-primary hover:underline">{text || "—"}</span>
              <span className="block font-mono text-xs text-muted">{record.employeeNumber}</span>
            </button>
          ),
        },
        {
          name: "transferKind",
          label: "Kind",
          gridHighlight: true,
          render: (text: string) => (text ? <Badge>{text}</Badge> : dash),
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
        {
          name: "effectiveDate",
          label: "Effective Date",
          sort: true,
          responsive: "md",
          render: (v: unknown) => {
            const d = fmtDate(v);
            return d ? <span className="tabular-nums">{d}</span> : dash;
          },
        },
        {
          name: "requestedByName",
          label: "Requested By",
          responsive: "lg",
          render: (v: string) => v || dash,
        },
        {
          name: "status",
          label: "Status",
          gridHighlight: true,
          render: (text: string) =>
            text ? <Badge variant={STATUS_VARIANT[text] ?? "muted"}>{text}</Badge> : dash,
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          // ⚠️ FOUR of these five hovers emitted nothing: `hover:bg-primary/10`,
          // `hover:bg-success/10`, `hover:bg-warning/10` and `hover:bg-secondary/40` are all
          // unregistered. Only Delete's `hover:bg-error/10` happened to be in theme.css, so one
          // button in five responded and the rest looked inert. All five use the same
          // core-Tailwind opacity now, so they behave identically.
          render: (_t: unknown, r: EmployeeMovementModel) => {
            const pending = r.status === "Pending";
            const executable = r.status === "Pending" || r.status === "Approved";
            const btn =
              "rounded p-1 transition-opacity hover:opacity-70 disabled:cursor-not-allowed disabled:opacity-40";
            return (
              <span className="inline-flex items-center gap-0.5">
                <button type="button" title={t("Edit") ?? ""} disabled={!pending}
                  onClick={() => r.id && editHandler(r.id)}
                  className={`${btn} text-primary`}>
                  <Pencil size={15} />
                </button>
                <button type="button" title={t("Execute now") ?? ""} disabled={!executable}
                  onClick={() => r.id && runAction(() => executeMovement(r.id!))}
                  className={`${btn} text-success`}>
                  <Play size={15} />
                </button>
                <button type="button" title={t("Cancel") ?? ""} disabled={!executable}
                  onClick={() => r.id && runAction(() => cancelMovement(r.id!))}
                  className={`${btn} text-warning`}>
                  <Ban size={15} />
                </button>
                <button type="button" title={t("Transfer notice") ?? ""}
                  onClick={() => setNoticeFor(r)}
                  className={`${btn} text-foreground`}>
                  <FileText size={15} />
                </button>
                <button type="button" title={t("Delete") ?? ""} disabled={r.status === "Completed"}
                  onClick={() => r.id && list.deleteRecord(r.id)}
                  className={`${btn} text-error`}>
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
      {action && (
        <div className="px-3 pt-2">
          {/* border-success/20 and border-error/20 are the registered widths — /30 and /40 are not. */}
          <p
            className={`flex items-start justify-between gap-2 rounded-md border px-3 py-2 text-xs ${
              action.ok
                ? "border-success/20 bg-success/15 text-success"
                : "border-error/20 bg-error/15 text-error"
            }`}
          >
            <span>{action.message}</span>
            <button
              type="button"
              onClick={() => setAction(null)}
              className="shrink-0 font-semibold transition-opacity hover:opacity-70"
              aria-label={t("Dismiss") ?? "Dismiss"}
            >
              ×
            </button>
          </p>
        </div>
      )}
      <EntityListShell
        listKey="transferRequests"
        listLabel="Transfer Requests"
        columns={columns}
        listFilters={STATUS_FILTER}
        {...list}
      />
      {noticeFor?.id && <NoticeModal movement={noticeFor} onClose={() => setNoticeFor(null)} />}
    </>
  );
}

export default TransferRequestList;
