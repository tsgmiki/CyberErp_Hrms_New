"use client";
import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Ban } from "lucide-react";
import { getAllOtherLeaves, cancelOtherLeave } from "@/services/admin/otherLeave";
import type { OtherLeaveModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { leaveStatusOptions, leaveStatusTone } from "@/constants/leave";

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

/** Earliest start → latest end across a request's detail rows. */
const periodSummary = (r: OtherLeaveModel) => {
  const rows = r.details ?? [];
  if (!rows.length) return "—";
  const starts = rows.map((d) => fmt(d.startDate)).filter(Boolean).sort();
  const ends = rows.map((d) => fmt(d.endDate)).filter(Boolean).sort();
  if (!starts.length || !ends.length) return "—";
  const from = starts[0];
  const to = ends[ends.length - 1];
  return from === to ? from : `${from} → ${to}`;
};

interface Props {
  editHandler: (id: string) => void;
  /** When set (Employee tab), the list is filtered to this employee and the Employee column is hidden. */
  employeeId?: string;
}

function OtherLeaveList({ editHandler, employeeId }: Props) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState("");
  const scoped = !!employeeId;

  // Same base key for global and per-employee lists; the employeeId in `param` differentiates the
  // cache entry, and an ["otherLeaves"] prefix-invalidation (from the form) refreshes both.
  const list = useEntityList({
    queryKey: "otherLeaves",
    fetchPage: getAllOtherLeaves,
    initialParam: { status, ...(employeeId ? { employeeId } : {}) },
  });
  const { setParam } = list;

  const onStatus = (v: string) => {
    setStatus(v);
    setParam((p) => ({ ...p, status: v, skip: 0 }));
  };

  const doCancel = async (id: string) => {
    if (!window.confirm("Cancel this leave request?")) return;
    await cancelOtherLeave(id);
    queryClient.invalidateQueries({ queryKey: ["otherLeaves"] });
    queryClient.invalidateQueries({ queryKey: ["otherLeaveBalances"] });
  };

  const columns = useMemo(
    () =>
      [
        ...(scoped
          ? []
          : [
              {
                name: "employeeName", label: "Employee", sort: true,
                render: (v: string, r: OtherLeaveModel) => (
                  <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
                    <span className="block font-semibold">{v ?? "—"}</span>
                    <span className="block text-xs text-muted">{r.employeeNumber}</span>
                  </button>
                ),
              },
            ]),
        {
          name: "leaveName", label: "Leave",
          render: (v: string, r: OtherLeaveModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              {v}
              {r.isLumpSum && <span className="ml-1.5 rounded bg-primary/10 px-1.5 py-0.5 text-[10px] font-semibold text-primary">ONE BLOCK</span>}
            </button>
          ),
        },
        { name: "requestDate", label: "Requested", render: (v: string) => fmt(v) },
        { name: "details", label: "Period", render: (_v: unknown, r: OtherLeaveModel) => periodSummary(r) },
        { name: "totalLeaveDays", label: "Days", render: (v: number) => (
          <span className="block text-right tabular-nums">{v}</span>) },
        {
          name: "status", label: "Status",
          render: (v: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${leaveStatusTone[v] ?? "bg-muted/30 text-muted"}`}>{v}</span>
          ),
        },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: OtherLeaveModel) =>
            r.status === "Pending" || r.status === "Approved" ? (
              <button
                type="button"
                title="Cancel"
                onClick={() => r.id && doCancel(r.id)}
                className="rounded p-1 text-warning transition-colors hover:bg-warning/10"
              >
                <Ban size={15} />
              </button>
            ) : null,
        },
      ] as DataTableColumnModel[],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [editHandler, scoped],
  );

  return (
    <div className="space-y-2">
      <div className="flex justify-end">
        <select
          className="h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground"
          value={status}
          onChange={(e) => onStatus(e.target.value)}
        >
          {leaveStatusOptions.map((o) => (
            <option key={o.id} value={o.id}>{o.name}</option>
          ))}
        </select>
      </div>
      <EntityListShell listKey="otherLeaves" listLabel="Other Leave Requests" columns={columns} {...list} />
    </div>
  );
}
export default OtherLeaveList;
