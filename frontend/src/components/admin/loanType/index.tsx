import { lazy, memo } from "react";
import { Landmark } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const LoanTypeForm = memo(lazy(() => import("./form")));
const LoanTypeList = memo(lazy(() => import("./list")));

function LoanType() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Loan Types"
      headerDescription="Staff-loan products with their limits, interest and service commitment"
      headerIcon={<Landmark className="h-6 w-6 text-primary" />}
      tableTitle="Loan Types"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<LoanTypeForm id={id} setId={setId} />}
      list={<LoanTypeList editHandler={editHandler} />}
    />
  );
}

export default LoanType;
