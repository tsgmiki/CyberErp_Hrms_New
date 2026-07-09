import { lazy, memo } from "react";
import { UserPlus } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const HiringRequestForm = memo(lazy(() => import("./form")));
const HiringRequestList = memo(lazy(() => import("./list")));

function HiringRequest() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Hiring Requests"
      headerDescription="Hiring Need Assessment — justified, budgeted requests approved before recruitment starts (HC077–HC083)"
      headerIcon={<UserPlus className="h-6 w-6 text-primary" />}
      tableTitle="Hiring Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<HiringRequestForm id={id} setId={setId} />}
      list={<HiringRequestList editHandler={editHandler} />}
    />
  );
}

export default HiringRequest;
