import { lazy, memo, useState } from "react";
import { Wallet } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import UtilizationModal from "./utilizationModal";

const TripBudgetForm = memo(lazy(() => import("./form")));
const TripBudgetList = memo(lazy(() => import("./list")));

function TripBudget() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  const [utilId, setUtilId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Travel Budgets"
        headerDescription="Annual travel allocations per department or organization-wide"
        headerIcon={<Wallet className="h-6 w-6 text-primary" />}
        tableTitle="Travel Budgets"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<TripBudgetForm id={id} setId={setId} />}
        list={<TripBudgetList editHandler={editHandler} utilizationHandler={setUtilId} />}
      />
      {utilId && <UtilizationModal budgetId={utilId} onClose={() => setUtilId(null)} />}
    </>
  );
}

export default TripBudget;
