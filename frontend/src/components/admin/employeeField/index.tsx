import { lazy, memo } from "react";
import { ListPlus } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const EmployeeFieldForm = memo(lazy(() => import("./form")));
const EmployeeFieldList = memo(lazy(() => import("./list")));

function EmployeeField() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Custom Fields"
      headerDescription="Add dynamic fields to the Employee form or any child form — no code changes"
      headerIcon={<ListPlus className="h-6 w-6 text-primary" />}
      tableTitle="Custom Fields"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<EmployeeFieldForm id={id} setId={setId} />}
      list={<EmployeeFieldList editHandler={editHandler} />}
    />
  );
}

export default EmployeeField;
