"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllLearningAssignment from "@/services/admin/learningAssignment/getAll";
import deleteLearningAssignment from "@/services/admin/learningAssignment/delete";
import type { LearningAssignmentModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function LearningAssignmentList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "learningAssignments",
    fetchPage: getAllLearningAssignment,
    deleteById: deleteLearningAssignment,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: LearningAssignmentModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "courseName", label: "Course" },
        {
          name: "audienceName",
          label: "Applies To",
          render: (v: string, record: LearningAssignmentModel) =>
            v ? `${v}${record.includeSubUnits ? " (+ sub-units)" : ""}` : "—",
        },
        {
          name: "dueWithinDays",
          label: "Deadline",
          render: (v: number, record: LearningAssignmentModel) =>
            record.recurrenceMonths
              ? `${v} days, every ${record.recurrenceMonths} months`
              : `${v} days, once`,
        },
        {
          name: "obligationCount",
          label: "Coverage",
          // The point of the grid: is this rule being met, and by how many people?
          render: (v: number, record: LearningAssignmentModel) =>
            v > 0 ? `${record.compliantCount ?? 0}/${v}` : "—",
        },
        {
          name: "overdueCount",
          label: "Overdue",
          render: (v: number) => (v > 0 ? v : "—"),
        },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: LearningAssignmentModel) => (
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
    <EntityListShell listKey="learningAssignments" listLabel="Mandatory Training" columns={columns} {...list} />
  );
}

export default LearningAssignmentList;
