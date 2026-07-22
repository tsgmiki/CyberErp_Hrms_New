import { lazy, memo } from "react";
import { ClipboardCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ClearanceDepartmentForm = memo(lazy(() => import("./form")));
const ClearanceDepartmentList = memo(lazy(() => import("./list")));

function ClearanceDepartment() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Clearance Departments"
      headerDescription="Configure the termination clearance checklist — each department with its authorized approvers (any one of them clears it)"
      headerIcon={<ClipboardCheck className="h-6 w-6 text-primary" />}
      tableTitle="Clearance Departments"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ClearanceDepartmentForm id={id} setId={setId} />}
      list={<ClearanceDepartmentList editHandler={editHandler} />}
    />
  );
}

export default ClearanceDepartment;
