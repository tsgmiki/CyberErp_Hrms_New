import { lazy, memo } from "react";
import { Landmark } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const LoanTypeForm = memo(lazy(() => import("./form")));
const LoanTypeList = memo(lazy(() => import("./list")));

function LoanType() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/loanType");

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
