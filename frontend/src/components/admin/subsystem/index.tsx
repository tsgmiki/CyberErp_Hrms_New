import { lazy, memo } from "react";
import { Boxes } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const SubsystemForm = memo(lazy(() => import("./subsystemForm")));
const SubsystemList = memo(lazy(() => import("./subsystemList")));

function Subsystem() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Subsystems"
      headerDescription="Master list of ERP subsystems — menu modules attach to one"
      headerIcon={<Boxes className="h-6 w-6 text-primary" />}
      tableTitle="Subsystems"
      tableDescription="Manage the subsystem master list (coreSubsystem)"
      tableIcon={<Boxes className="h-5 w-5 text-primary" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<SubsystemForm id={id} setSubsystemId={setId} />}
      list={<SubsystemList editHandler={editHandler} />}
    />
  );
}

export default Subsystem;
