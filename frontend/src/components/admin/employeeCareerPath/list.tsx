"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllEmployeeCareerPath from "@/services/admin/employeeCareerPath/getAll";
import deleteEmployeeCareerPath from "@/services/admin/employeeCareerPath/delete";
import type { EmployeeCareerPathModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const STATUS_TONE: Record<string, string> = {
  Active: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  OnHold: "bg-warning/15 text-warning",
};

function EmployeeCareerPathList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "employeeCareerPaths",
    fetchPage: getAllEmployeeCareerPath,
    deleteById: deleteEmployeeCareerPath,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName", label: "Employee", sort: true,
          render: (t: string, r: EmployeeCareerPathModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{t ?? "—"}</span>
              <span className="block text-xs text-muted">{r.employeeNumber}</span>
            </button>
          ),
        },
        { name: "careerPathName", label: "Career Path" },
        {
          name: "progressPercent", label: "Progress",
          render: (v: number) => (
            <div className="flex items-center gap-2">
              <div className="h-1.5 w-20 overflow-hidden rounded-full bg-secondary">
                <div className="h-full rounded-full bg-primary" style={{ width: `${Number(v ?? 0)}%` }} />
              </div>
              <span className="text-xs tabular-nums text-muted">{Number(v ?? 0).toFixed(0)}%</span>
            </div>
          ),
        },
        { name: "status", label: "Status", render: (v: string) => (<span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>{v}</span>) },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: EmployeeCareerPathModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="employeeCareerPaths" listLabel="Assignments" columns={columns} {...list} />;
}

export default EmployeeCareerPathList;
