"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllAppraisals, deleteAppraisal } from "@/services/admin/appraisal";
import type { AppraisalModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import { appraisalStageLabel } from "@/constants/performance";

interface Props {
  editHandler: (id: string) => void;
}

function AppraisalList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "appraisals",
    fetchPage: getAllAppraisals,
    deleteById: deleteAppraisal,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "employeeName",
          label: "Employee",
          sort: true,
          render: (text: string, record: AppraisalModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "reviewCycleName", label: "Review Cycle" },
        { name: "stage", label: "Stage", render: (v: string) => appraisalStageLabel(v) },
        { name: "overallScore", label: "Overall", render: (v: number | null) => (v == null ? "—" : String(v)) },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: AppraisalModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return (
    <EntityListShell listKey="appraisals" listLabel="Appraisals" columns={columns} {...list} />
  );
}

export default AppraisalList;
