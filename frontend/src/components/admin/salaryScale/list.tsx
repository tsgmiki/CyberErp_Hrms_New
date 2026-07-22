"use client";

import { useEffect, useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllSalaryScale from "@/services/admin/salaryScale/getAll";
import deleteSalaryScale from "@/services/admin/salaryScale/delete";
import type { SalaryScaleModel, JobGradeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
  jobGradeId: string;
  onSelectJobGrade: (id: string) => void;
  jobGrades: JobGradeModel[];
}

function SalaryScaleList({ editHandler, jobGradeId, onSelectJobGrade, jobGrades }: Props) {
  const list = useEntityList({
    queryKey: "salaryScales",
    fetchPage: getAllSalaryScale,
    deleteById: deleteSalaryScale,
    initialParam: { jobGradeId },
  });

  // Keep the paged query scoped to the currently-selected job grade.
  const { setParam } = list;
  useEffect(() => {
    setParam((p) => ({ ...p, jobGradeId, skip: 0 }));
  }, [jobGradeId, setParam]);

  const columns = useMemo(
    () =>
      [
        {
          name: "step",
          label: "Step",
          sort: true,
          render: (text: string, record: SalaryScaleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="font-semibold"
            >
              {text}
            </button>
          ),
        },
        {
          name: "salary",
          label: "Salary",
          render: (_t: unknown, record: SalaryScaleModel) =>
            record.salary != null
              ? Number(record.salary).toLocaleString(undefined, { minimumFractionDigits: 2 })
              : "",
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: SalaryScaleModel) => (
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
    <div className="space-y-4">
      {/* Job Grade filter — the grid only shows data once a grade is chosen. */}
      <div className="max-w-md">
        <label className="mb-1 block text-sm font-medium text-muted">Job Grade</label>
        <select
          value={jobGradeId}
          onChange={(e) => onSelectJobGrade(e.target.value)}
          className="h-9 w-full rounded-lg border border-border bg-background px-3 text-sm text-foreground outline-none focus:border-primary"
        >
          <option value="">Select a job grade…</option>
          {jobGrades.map((g) => (
            <option key={g.id} value={g.id}>
              {g.name}
            </option>
          ))}
        </select>
      </div>

      {jobGradeId ? (
        <EntityListShell
          listKey="salaryScales"
          listLabel="Salary Scale"
          columns={columns}
          {...list}
        />
      ) : (
        <div className="rounded-md border border-dashed border-border p-8 text-center text-sm text-muted">
          Select a job grade above to view and manage its salary scale.
        </div>
      )}
    </div>
  );
}

export default SalaryScaleList;
