"use client";

import { useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Printer } from "lucide-react";
import getAllAnnualLeave from "@/services/admin/annualLeave/getAll";
import cancelAnnualLeave from "@/services/admin/annualLeave/cancel";
import type { AnnualLeaveModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { leaveStatusOptions, leaveStatusTone } from "@/constants/leave";
import GenerateAnnualLeaveModal from "./generateAnnualLeaveModal";

interface Props {
  editHandler: (id: string) => void;
  /** When set (Employee tab), the list is filtered to this employee and the Employee column is hidden. */
  employeeId?: string;
}

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

/** Earliest start → latest end across a request's detail rows. */
const periodSummary = (r: AnnualLeaveModel) => {
  const rows = r.details ?? [];
  if (!rows.length) return "—";
  const starts = rows.map((d) => fmt(d.startDate)).filter(Boolean).sort();
  const ends = rows.map((d) => fmt(d.endDate)).filter(Boolean).sort();
  if (!starts.length || !ends.length) return "—";
  const from = starts[0];
  const to = ends[ends.length - 1];
  return from === to ? from : `${from} → ${to}`;
};

function AnnualLeaveList({ editHandler, employeeId }: Props) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState("");
  const [printFor, setPrintFor] = useState<AnnualLeaveModel | null>(null);
  const scoped = !!employeeId;

  // Same base key for global and per-employee lists; the employeeId in `param` differentiates the
  // cache entry, and an ["annualLeaves"] prefix-invalidation (from the form) refreshes both.
  const list = useEntityList({
    queryKey: "annualLeaves",
    fetchPage: getAllAnnualLeave,
    initialParam: { status, ...(employeeId ? { employeeId } : {}) },
  });
  const { setParam } = list;

  const onStatus = (v: string) => {
    setStatus(v);
    setParam((p) => ({ ...p, status: v, skip: 0 }));
  };

  const doCancel = async (id: string) => {
    if (!window.confirm("Cancel this annual leave request?")) return;
    await cancelAnnualLeave(id);
    queryClient.invalidateQueries({ queryKey: ["annualLeaves"] });
  };

  const columns = useMemo(
    () =>
      [
        // Employee column only in the global list; in the Employee tab it's redundant, so Fiscal Year
        // is the clickable column that opens the request detail.
        ...(scoped
          ? [
              {
                name: "fiscalYearName",
                label: "Fiscal Year",
                render: (_t: unknown, r: AnnualLeaveModel) => (
                  <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
                    {r.fiscalYearName || "—"}
                  </button>
                ),
              },
            ]
          : [
              {
                name: "employeeName",
                label: "Employee",
                render: (_t: unknown, r: AnnualLeaveModel) => (
                  <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
                    {r.employeeName || r.employeeNumber || "—"}
                  </button>
                ),
              },
              { name: "fiscalYearName", label: "Fiscal Year", render: (_t: unknown, r: AnnualLeaveModel) => r.fiscalYearName || "—" },
            ]),
        { name: "period", label: "Period", render: (_t: unknown, r: AnnualLeaveModel) => periodSummary(r) },
        { name: "totalLeaveDays", label: "Days", render: (_t: unknown, r: AnnualLeaveModel) => String(r.totalLeaveDays ?? "") },
        {
          name: "status",
          label: "Status",
          render: (_t: unknown, r: AnnualLeaveModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${leaveStatusTone[r.status || ""] || ""}`}>
              {r.status}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: AnnualLeaveModel) => (
            <div className="flex items-center gap-1.5">
              <button
                type="button"
                onClick={() => setPrintFor(r)}
                title="Print / export this request"
                className="inline-flex items-center gap-1 rounded-md border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
              >
                <Printer size={13} /> Print
              </button>
              {(r.status === "Pending" || r.status === "Approved") && (
                <button
                  type="button"
                  onClick={() => r.id && doCancel(r.id)}
                  className="rounded-md border border-border px-2 py-1 text-xs text-error hover:bg-error/10"
                >
                  Cancel
                </button>
              )}
            </div>
          ),
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
      <EntityListShell listKey="annualLeaves" listLabel="Annual Leave Requests" columns={columns} {...list} />

      {printFor?.id && (
        <GenerateAnnualLeaveModal
          annualLeaveId={printFor.id}
          label={printFor.employeeName || printFor.fiscalYearName || undefined}
          onClose={() => setPrintFor(null)}
        />
      )}
    </div>
  );
}

export default AnnualLeaveList;
