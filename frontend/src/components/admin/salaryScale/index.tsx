import { lazy, memo, useMemo, useState } from "react";
import { Coins, Info } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { EntityModuleShell, useEntityRouteModule } from "@/template";
import ButtonField from "@/components/ui/buttonField";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import { parameterInitialData } from "@/constants/initialization";
import type { JobGradeModel } from "@/models";

const SalaryScaleForm = memo(lazy(() => import("./form")));
const SalaryScaleList = memo(lazy(() => import("./list")));

/**
 * Shown instead of the create form when no job grade has been chosen. Unlike the Positions screen —
 * which keeps its org tree on screen next to the hint — this module SWAPS list for form, so the grade
 * picker is not reachable from here; the hint therefore carries its own way back.
 */
function GradeRequiredHint({ onBack }: { onBack: () => void }) {
  const { t } = useTranslation();
  return (
    <div className="flex min-h-[40vh] flex-col items-center justify-center gap-3 p-6 text-center">
      <Info className="h-10 w-10 text-info" />
      <h2 className="text-base font-semibold text-foreground">{t("Choose a job grade first")}</h2>
      <p className="max-w-md text-sm text-muted">
        {t("A salary scale row is a salary for one step of one job grade. Go back to the list, pick a job grade, then click Add.")}
      </p>
      <ButtonField value="Back to list" variant="outline" onClick={onBack} />
    </div>
  );
}

function SalaryScale() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/salaryScale");
  const [jobGradeId, setJobGradeId] = useState("");

  const [gradeParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: jobGrades } = useQuery({
    queryKey: ["jobGrades", gradeParam],
    queryFn: () => getAllJobGrade(gradeParam),
  });
  const grades: JobGradeModel[] = useMemo(() => jobGrades?.data ?? [], [jobGrades]);
  const selectedGrade = useMemo(
    () => grades.find((g) => g.id === jobGradeId),
    [grades, jobGradeId],
  );
  const gradeLabel = selectedGrade
    ? `${selectedGrade.code} — ${selectedGrade.name}`
    : "";

  // The grade a new row belongs to comes from the list's filter, which is local state — it is NOT in
  // the URL. So a cold /salaryScale/new (pasted link, refresh, or Back into it) has no grade to
  // attach the row to, and its hidden jobGradeId would go out empty for the API to reject with
  // "Job grade is required". Editing is unaffected: an existing row carries its own grade.
  const needsGrade = showForm && !id && !jobGradeId;

  return (
    <EntityModuleShell
      title="Salary Scale"
      headerDescription="Define salary amounts per step for each job grade"
      headerIcon={<Coins className="h-6 w-6 text-primary" />}
      tableTitle="Salary Scale"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      hideAdd={!jobGradeId}
      form={
        needsGrade ? (
          <GradeRequiredHint onBack={backHandler} />
        ) : (
          <SalaryScaleForm
            id={id}
            setId={setId}
            jobGradeId={jobGradeId}
            gradeLabel={gradeLabel}
          />
        )
      }
      list={
        <SalaryScaleList
          editHandler={editHandler}
          jobGradeId={jobGradeId}
          onSelectJobGrade={setJobGradeId}
          jobGrades={grades}
        />
      }
    />
  );
}

export default SalaryScale;
