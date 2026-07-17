"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllDevelopmentPlans, deleteDevelopmentPlan } from "@/services/admin/developmentPlan";
import type { DevelopmentPlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function DevelopmentPlanList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "developmentPlans",
    fetchPage: getAllDevelopmentPlans,
    deleteById: deleteDevelopmentPlan,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Title",
          sort: true,
          render: (text: string, record: DevelopmentPlanModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee" },
        { name: "actions", label: "Actions", render: (v: unknown) => (Array.isArray(v) ? v.length : 0) },
        { name: "status", label: "Status" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: DevelopmentPlanModel) => (
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
    <EntityListShell listKey="developmentPlans" listLabel="Development Plans" columns={columns} {...list} />
  );
}

export default DevelopmentPlanList;
