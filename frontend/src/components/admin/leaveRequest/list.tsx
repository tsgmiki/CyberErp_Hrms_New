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
}

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

function LeaveRequestList({ editHandler }: Props) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState("");

  const list = useEntityList({
    queryKey: "leaveRequests",
    fetchPage: getAllLeaveRequest,
    initialParam: { status },
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
        {
          name: "employeeName",
          label: "Employee",
          render: (_t: unknown, r: LeaveRequestModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
              {r.employeeName || r.employeeNumber || "—"}
            </button>
          ),
        },
        { name: "leaveTypeCode", label: "Type", render: (_t: unknown, r: LeaveRequestModel) => r.leaveTypeName || r.leaveTypeCode },
        { name: "startDate", label: "From", render: (_t: unknown, r: LeaveRequestModel) => fmt(r.startDate) },
        { name: "endDate", label: "To", render: (_t: unknown, r: LeaveRequestModel) => fmt(r.endDate) },
        { name: "workingDays", label: "Days", render: (_t: unknown, r: LeaveRequestModel) => String(r.workingDays ?? "") },
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
    [editHandler],
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
