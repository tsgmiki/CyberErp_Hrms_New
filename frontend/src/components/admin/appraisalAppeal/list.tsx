"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllAppraisalAppeals } from "@/services/admin/appraisalAppeal";
import type { AppraisalAppealModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmtDate = (v: unknown) => (v ? String(v).slice(0, 10) : "—");

function AppraisalAppealList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "appraisalAppeals",
    fetchPage: getAllAppraisalAppeals,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: AppraisalAppealModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "status", label: "Status" },
        { name: "requestFollowUp", label: "Follow-up", render: (v: boolean) => (v ? "Yes" : "No") },
        { name: "resolvedAt", label: "Resolved", render: fmtDate },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AppraisalAppealModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete={false}
              editHandler={editHandler}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler],
  );

  return (
    <EntityListShell listKey="appraisalAppeals" listLabel="Appeals" columns={columns} {...list} />
  );
}

export default AppraisalAppealList;
