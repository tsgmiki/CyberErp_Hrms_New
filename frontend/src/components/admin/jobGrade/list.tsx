"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import deleteJobGrade from "@/services/admin/jobGrade/delete";
import type { JobGradeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function JobGradeList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "jobGrades",
    fetchPage: getAllJobGrade,
    deleteById: deleteJobGrade,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "code",
          label: "Code",
          sort: true,
          render: (text: string, record: JobGradeModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        { name: "name", label: "Name", sort: true },
        { name: "nameA", label: "Name (Amharic)" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: JobGradeModel) => (
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
    <EntityListShell listKey="jobGrades" listLabel="Job Grades" columns={columns} {...list} />
  );
}

export default JobGradeList;
