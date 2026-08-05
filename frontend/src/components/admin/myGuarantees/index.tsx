import { lazy, memo } from "react";
import { Handshake } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const GuaranteeForm = memo(lazy(() => import("../employeeGuarantee/form")));
const GuaranteeList = memo(lazy(() => import("../employeeGuarantee/list")));

/** §3.12 HC306 — the signed-in employee records and amends their OWN guarantee commitments. */
function MyGuarantees() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  return (
    <EntityModuleShell
      title="My Guarantees"
      headerDescription="Record and track your guarantee commitments toward external organizations"
      headerIcon={<Handshake className="h-6 w-6 text-primary" />}
      tableTitle="My Guarantee Commitments"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<GuaranteeForm id={id} setId={setId} mine />}
      list={<GuaranteeList editHandler={editHandler} mine />}
    />
  );
}
export default MyGuarantees;
