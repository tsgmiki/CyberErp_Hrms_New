import { lazy, memo } from "react";
import { Handshake } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const GuaranteeForm = memo(lazy(() => import("./form")));
const GuaranteeList = memo(lazy(() => import("./list")));

/** §3.12 HC305–HC307 — HR register of employee guarantee commitments toward external organizations. */
function EmployeeGuarantee() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/employeeGuarantee");
  return (
    <EntityModuleShell
      title="Guarantee Register"
      headerDescription="Employee guarantee commitments for external organizations (NBE procedures)"
      headerIcon={<Handshake className="h-6 w-6 text-primary" />}
      tableTitle="Guarantee Commitments"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<GuaranteeForm id={id} setId={setId} />}
      list={<GuaranteeList editHandler={editHandler} />}
    />
  );
}
export default EmployeeGuarantee;
