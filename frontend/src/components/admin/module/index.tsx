import { lazy, memo } from "react";
import { LayoutGrid } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ModuleForm = memo(lazy(() => import("./moduleForm")));
const ModuleList = memo(lazy(() => import("./moduleList")));

function Module() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Modules"
      headerDescription="Manage application modules and subsystems"
      headerIcon={<LayoutGrid className="h-6 w-6 text-primary" />}
      tableTitle="Modules"
      tableDescription="Manage application modules and their permissions"
      tableIcon={<LayoutGrid className="h-5 w-5 text-primary" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ModuleForm id={id} setModuleId={setId} />}
      list={<ModuleList editHandler={editHandler} />}
    />
  );
}

export default Module;
