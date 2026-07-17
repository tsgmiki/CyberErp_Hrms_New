import { lazy, memo } from "react";
import { Target } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const EmployeeGoalForm = memo(lazy(() => import("./form")));
const EmployeeGoalList = memo(lazy(() => import("./list")));

function EmployeeGoal() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Employee Goals"
      headerDescription="Set SMART goals and action plans, aligned to objectives"
      headerIcon={<Target className="h-6 w-6 text-primary" />}
      tableTitle="Employee Goals"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<EmployeeGoalForm id={id} setId={setId} />}
      list={<EmployeeGoalList editHandler={editHandler} />}
    />
  );
}

export default EmployeeGoal;
