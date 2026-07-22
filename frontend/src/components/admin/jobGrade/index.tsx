import { lazy, memo } from "react";
import { Layers } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const JobGradeForm = memo(lazy(() => import("./form")));
const JobGradeList = memo(lazy(() => import("./list")));

function JobGrade() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Job Grades"
      headerDescription="Manage job grades and pay bands"
      headerIcon={<Layers className="h-6 w-6 text-primary" />}
      tableTitle="Job Grades"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<JobGradeForm id={id} setId={setId} />}
      list={<JobGradeList editHandler={editHandler} />}
    />
  );
}

export default JobGrade;
