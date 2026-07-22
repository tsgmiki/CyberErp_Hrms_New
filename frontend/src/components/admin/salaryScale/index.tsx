import { lazy, memo, useMemo, useState } from "react";
import { Coins } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import { parameterInitialData } from "@/constants/initialization";
import type { JobGradeModel } from "@/models";

const SalaryScaleForm = memo(lazy(() => import("./form")));
const SalaryScaleList = memo(lazy(() => import("./list")));

function SalaryScale() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();
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
        <SalaryScaleForm
          id={id}
          setId={setId}
          jobGradeId={jobGradeId}
          gradeLabel={gradeLabel}
        />
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
