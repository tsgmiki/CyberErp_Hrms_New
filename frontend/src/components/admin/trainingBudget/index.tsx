import { lazy, memo } from "react";
import { Wallet } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingBudgetForm = memo(lazy(() => import("./form")));
const TrainingBudgetList = memo(lazy(() => import("./list")));

function TrainingBudget() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Training Budgets"
      headerDescription="Budget envelopes per fiscal year with live utilization"
      headerIcon={<Wallet className="h-6 w-6 text-primary" />}
      tableTitle="Training Budgets"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingBudgetForm id={id} setId={setId} />}
      list={<TrainingBudgetList editHandler={editHandler} />}
    />
  );
}

export default TrainingBudget;
