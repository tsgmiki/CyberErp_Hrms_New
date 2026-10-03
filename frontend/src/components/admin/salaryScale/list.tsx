"use client";

import { useEffect, useMemo } from "react";
import { Coins } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import EmptyState from "../../common/emptyState";
import DropDownField from "@/components/ui/dropDownField";
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

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

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

  // `code — name`, matching the label the form shows for the same grade, so the two screens name
  // a grade identically rather than one saying "01" and the other "001 — 01".
  const gradeOptions = useMemo(
    () =>
      jobGrades.map((g) => ({
        id: g.id,
        name: g.code ? `${g.code} — ${g.name}` : (g.name ?? ""),
      })),
    [jobGrades],
  );

  const selectedGradeLabel = useMemo(
    () => gradeOptions.find((g) => g.id === jobGradeId)?.name ?? "",
    [gradeOptions, jobGradeId],
  );

  const columns = useMemo(
    () =>
      [
        {
          name: "step",
          label: "Step",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: SalaryScaleModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left font-semibold text-primary hover:underline"
            >
              {text || dash}
            </button>
          ),
        },
        {
          name: "salary",
          label: "Salary",
          sort: true,
          gridHighlight: true,
          // ⚠️ Was an empty string for a missing salary. A pay scale row with no amount is the one
          // thing somebody needs to SEE, not a cell that looks like the table failed to render.
          render: (_t: unknown, record: SalaryScaleModel) =>
            record.salary == null ? dash : (
              <span className="font-medium tabular-nums">
                {Number(record.salary).toLocaleString(undefined, { minimumFractionDigits: 2 })}
              </span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
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
      {/*
        ⚠️ The grade picker is the standard DropDownFieldV2, not a raw <select>. The project's UI
        standard is explicit that admin screens never hand-roll input/select/textarea/table, and a
        bare <select> here was both the one unstyled control on the screen and unsearchable — which
        matters at 38 grades and would only get worse.
      */}
      <div className="max-w-md">
        <DropDownField
          name="jobGradeFilter"
          type="dropDown"
          label="Job Grade"
          labelWidth="w-[30%]"
          placeholder="Select a job grade…"
          value={jobGradeId}
          displayValue={selectedGradeLabel}
          data={gradeOptions as never}
          onSelect={(_name: string, r: any) => onSelectJobGrade(r?.id ?? "")}
        />
      </div>

      {jobGradeId ? (
        <EntityListShell
          listKey="salaryScales"
          listLabel="Salary Scale"
          columns={columns}
          {...list}
        />
      ) : (
        <EmptyState
          icon={<Coins className="h-6 w-6" aria-hidden />}
          title="Choose a job grade"
          description="A salary scale row is one step of one job grade. Pick a grade above to see and manage its steps."
        />
      )}
    </div>
  );
}

export default SalaryScaleList;
