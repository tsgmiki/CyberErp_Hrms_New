"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import { getAllEmployeeGoals, deleteEmployeeGoal } from "@/services/admin/employeeGoal";
import type { EmployeeGoalModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function EmployeeGoalList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "employeeGoals",
    fetchPage: getAllEmployeeGoals,
    deleteById: deleteEmployeeGoal,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Goal",
          sort: true,
          render: (text: string, record: EmployeeGoalModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "employeeName", label: "Employee" },
        { name: "objectiveTitle", label: "Objective", render: (v: unknown) => (v ? String(v) : "—") },
        { name: "weight", label: "Weight", render: (v: number) => `${v ?? 0}%` },
        { name: "progressPercent", label: "Progress", render: (v: number) => `${v ?? 0}%` },
        { name: "status", label: "Status" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: EmployeeGoalModel) => (
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
    <EntityListShell listKey="employeeGoals" listLabel="Employee Goals" columns={columns} {...list} />
  );
}

export default EmployeeGoalList;
