"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
// ⚠️ No status filter here, unlike Position Classes and Positions: JobGradeDto is exactly
// { Id, Name, NameA, Code } — there is no active flag, no vacancy, nothing to filter on. A
// filter would have to invent a field the API does not return.
import deleteJobGrade from "@/services/admin/jobGrade/delete";
import type { JobGradeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

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
          name: "name",
          label: "Grade",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: JobGradeModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left font-semibold text-primary hover:underline"
            >
              {record.name || dash}
            </button>
          ),
        },
        {
          name: "code",
          label: "Code",
          sort: true,
          width: "w-32",
          render: (text: string) => (
            <span className="font-mono text-xs text-foreground">{text || "—"}</span>
          ),
        },
        {
          // ⚠️ Kept as its own column, NOT folded into the grade like the Amharic title on
          // Position Classes. That one was populated for 1 record in 814; this is populated for 16
          // of 38, so it is a real column carrying real data rather than an empty strip.
          name: "nameA",
          label: "Name (Amharic)",
          responsive: "md",
          render: (text: string) => text || dash,
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
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
