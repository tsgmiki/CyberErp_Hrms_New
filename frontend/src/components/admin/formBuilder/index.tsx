import { lazy, memo } from "react";
import { LayoutGrid } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const FormBuilderForm = memo(lazy(() => import("./form")));
const FormBuilderList = memo(lazy(() => import("./list")));

function FormBuilder() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Form Builder"
      headerDescription="Create custom tabs (forms) on the Employee profile — no developer needed"
      headerIcon={<LayoutGrid className="h-6 w-6 text-primary" />}
      tableTitle="Custom Forms"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<FormBuilderForm id={id} setId={setId} />}
      list={<FormBuilderList editHandler={editHandler} />}
    />
  );
}

export default FormBuilder;
