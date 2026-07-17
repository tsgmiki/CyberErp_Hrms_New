"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllImprovementPlans, deleteImprovementPlan } from "@/services/admin/improvementPlan";
import type { ImprovementPlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function ImprovementPlanList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "improvementPlans",
    fetchPage: getAllImprovementPlans,
    deleteById: deleteImprovementPlan,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Title",
          sort: true,
          render: (text: string, record: ImprovementPlanModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee" },
        { name: "status", label: "Status" },
        { name: "outcome", label: "Outcome", render: (v: string) => (v && v !== "Pending" ? v : "—") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: ImprovementPlanModel) => (
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
    <EntityListShell listKey="improvementPlans" listLabel="Improvement Plans" columns={columns} {...list} />
  );
}

export default ImprovementPlanList;
