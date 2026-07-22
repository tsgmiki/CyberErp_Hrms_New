"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import getAllLeaveRequest from "@/services/admin/leaveRequest/getAll";
import cancelLeaveRequest from "@/services/admin/leaveRequest/cancel";
import type { LeaveRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { leaveStatusOptions, leaveStatusTone } from "@/constants/leave";

interface Props {
  editHandler: (id: string) => void;
  /** When set (Employee tab), the list is filtered to this employee and the Employee column is hidden. */
  employeeId?: string;
}

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

/** Distinct leave-type labels across a request's lines. */
const typeSummary = (r: LeaveRequestModel) => {
  const names = (r.lines ?? [])
    .map((l) => l.leaveTypeName || l.leaveTypeCode)
    .filter((v, i, a): v is string => !!v && a.indexOf(v) === i);
  return names.length ? names.join(", ") : "—";
};

/** Earliest start → latest end across a request's lines. */
const periodSummary = (r: LeaveRequestModel) => {
  const lines = r.lines ?? [];
  if (!lines.length) return "—";
  const starts = lines.map((l) => fmt(l.startDate)).filter(Boolean).sort();
  const ends = lines.map((l) => fmt(l.endDate)).filter(Boolean).sort();
  if (!starts.length || !ends.length) return "—";
  const from = starts[0];
  const to = ends[ends.length - 1];
  return from === to ? from : `${from} → ${to}`;
};

function LeaveRequestList({ editHandler, employeeId }: Props) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState("");
  const scoped = !!employeeId;

  // Same base key for global and per-employee lists; the employeeId in `param` differentiates the
  // cache entry, and a ["leaveRequests"] prefix-invalidation (from the form) refreshes both.
  const list = useEntityList({
    queryKey: "leaveRequests",
    fetchPage: getAllLeaveRequest,
    initialParam: { status, ...(employeeId ? { employeeId } : {}) },
  });
  const { setParam } = list;

  const onStatus = (v: string) => {
    setStatus(v);
    setParam((p) => ({ ...p, status: v, skip: 0 }));
  };

  const doCancel = async (id: string) => {
    if (!window.confirm("Cancel this leave request?")) return;
    await cancelLeaveRequest(id);
    queryClient.invalidateQueries({ queryKey: ["leaveRequests"] });
  };

  const columns = useMemo(
    () =>
      [
        // Employee column only in the global list; in the Employee tab it's redundant, so Type(s) is
        // the clickable column that opens the request detail.
        ...(scoped
          ? [
              {
                name: "types",
                label: "Type(s)",
                render: (_t: unknown, r: LeaveRequestModel) => (
                  <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
                    {typeSummary(r)}
                  </button>
                ),
              },
            ]
          : [
              {
                name: "employeeName",
                label: "Employee",
                render: (_t: unknown, r: LeaveRequestModel) => (
                  <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
                    {r.employeeName || r.employeeNumber || "—"}
                  </button>
                ),
              },
              { name: "types", label: "Type(s)", render: (_t: unknown, r: LeaveRequestModel) => typeSummary(r) },
            ]),
        { name: "period", label: "Period", render: (_t: unknown, r: LeaveRequestModel) => periodSummary(r) },
        { name: "totalWorkingDays", label: "Days", render: (_t: unknown, r: LeaveRequestModel) => String(r.totalWorkingDays ?? "") },
        {
          name: "status",
          label: "Status",
          render: (_t: unknown, r: LeaveRequestModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${leaveStatusTone[r.status || ""] || ""}`}>
              {r.status}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: LeaveRequestModel) =>
            r.status === "Pending" || r.status === "Approved" ? (
              <button
                type="button"
                onClick={() => r.id && doCancel(r.id)}
                className="rounded-md border border-border px-2 py-1 text-xs text-error hover:bg-error/10"
              >
                Cancel
              </button>
            ) : null,
        },
      ] as DataTableColumnModel[],
    [editHandler, scoped],
  );

  return (
    <div className="space-y-4">
      <div className="max-w-xs">
        <label className="mb-1 block text-sm font-medium text-muted">Filter by status</label>
        <select
          value={status}
          onChange={(e) => onStatus(e.target.value)}
          className="h-9 w-full rounded-lg border border-border bg-background px-3 text-sm text-foreground outline-none focus:border-primary"
        >
          {leaveStatusOptions.map((o) => (
            <option key={o.id} value={o.id}>{o.name}</option>
          ))}
        </select>
      </div>
      <EntityListShell listKey="leaveRequests" listLabel="Leave Requests" columns={columns} {...list} />
    </div>
  );
}

export default LeaveRequestList;
