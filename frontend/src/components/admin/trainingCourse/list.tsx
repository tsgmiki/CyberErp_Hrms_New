"use client";

import { useMemo } from "react";
import { ExternalLink } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import getAllTrainingCourse from "@/services/admin/trainingCourse/getAll";
import deleteTrainingCourse from "@/services/admin/trainingCourse/delete";
import type { TrainingCourseModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function TrainingCourseList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "trainingCourses",
    fetchPage: getAllTrainingCourse,
    deleteById: deleteTrainingCourse,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Course",
          sort: true,
          render: (text: string, record: TrainingCourseModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="text-left">
              <span className="block font-semibold">{text}</span>
              <span className="block text-xs text-muted">{record.code}</span>
            </button>
          ),
        },
        { name: "categoryName", label: "Category", render: (v: string) => v || "—" },
        { name: "deliveryMode", label: "Delivery" },
        { name: "durationHours", label: "Hours", render: (v: number) => (v != null ? v : "—") },
        { name: "cpdHours", label: "CPD", render: (v: number) => (v ? v : "—") },
        {
          name: "providerName",
          label: "Provider",
          render: (v: string, record: TrainingCourseModel) =>
            record.isExternal ? (
              <span className="inline-flex items-center gap-1">
                {v}
                {record.externalUrl && (
                  <a href={record.externalUrl} target="_blank" rel="noreferrer" className="text-primary">
                    <ExternalLink size={12} />
                  </a>
                )}
              </span>
            ) : (
              "Internal"
            ),
        },
        { name: "isActive", label: "Active", render: (v: boolean) => (v ? "Yes" : "No") },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TrainingCourseModel) => (
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
    <EntityListShell listKey="trainingCourses" listLabel="Course Catalog" columns={columns} {...list} />
  );
}

export default TrainingCourseList;
