"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllCriticalPosition from "@/services/admin/criticalPosition/getAll";
import deleteCriticalPosition from "@/services/admin/criticalPosition/delete";
import type { CriticalPositionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const RISK_TONE: Record<string, string> = {
  High: "bg-error/15 text-error",
  Medium: "bg-warning/15 text-warning",
  Low: "bg-muted/30 text-muted",
};

function CriticalPositionList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "criticalPositions",
    fetchPage: getAllCriticalPosition,
    deleteById: deleteCriticalPosition,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "positionTitle", label: "Role / Position", sort: true,
          render: (_t: unknown, r: CriticalPositionModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{r.positionTitle ?? r.positionCode ?? "—"}</span>
              <span className="block text-xs text-muted">{r.positionCode}{r.organizationUnitName ? ` · ${r.organizationUnitName}` : ""}</span>
            </button>
          ),
        },
        {
          name: "riskLevel", label: "Risk",
          render: (v: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${RISK_TONE[v] ?? "bg-muted/30 text-muted"}`}>{v}</span>
          ),
        },
        { name: "reason", label: "Reason" },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: CriticalPositionModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="criticalPositions" listLabel="Critical Positions" columns={columns} {...list} />;
}

export default CriticalPositionList;
