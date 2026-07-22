"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllChangeRequest from "@/services/admin/careerPathChangeRequest/getAll";
import deleteChangeRequest from "@/services/admin/careerPathChangeRequest/delete";
import type { CareerPathChangeRequestModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const STATUS_TONE: Record<string, string> = {
  Draft: "bg-muted/30 text-muted",
  Submitted: "bg-info/15 text-info",
  Approved: "bg-success/15 text-success",
  Rejected: "bg-error/15 text-error",
};

function ChangeRequestList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({
    queryKey: "careerPathChangeRequests",
    fetchPage: getAllChangeRequest,
    deleteById: deleteChangeRequest,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName", label: "Employee", sort: true,
          render: (t: string, r: CareerPathChangeRequestModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{t ?? "—"}</span>
              <span className="block text-xs text-muted">{r.employeeNumber}</span>
            </button>
          ),
        },
        { name: "reason", label: "Reason" },
        { name: "status", label: "Status", render: (v: string) => (<span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>{v}</span>) },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: CareerPathChangeRequestModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="careerPathChangeRequests" listLabel="Change Requests" columns={columns} {...list} />;
}

export default ChangeRequestList;
