import { lazy, memo } from "react";
import { Building } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const BranchForm = memo(lazy(() => import("./form")));
const BranchList = memo(lazy(() => import("./list")));

function Branch() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Branches"
      headerDescription="Distinct branches / business units"
      headerIcon={<Building className="h-6 w-6 text-primary" />}
      tableTitle="Branches"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<BranchForm id={id} setId={setId} />}
      list={<BranchList editHandler={editHandler} />}
    />
  );
}

export default Branch;
