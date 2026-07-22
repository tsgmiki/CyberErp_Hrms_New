import { lazy, memo } from "react";
import { ClipboardList } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingNeedForm = memo(lazy(() => import("./form")));
const TrainingNeedList = memo(lazy(() => import("./list")));

function TrainingNeed() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Training Needs"
      headerDescription="Request training locally or abroad — approvals route through the workflow"
      headerIcon={<ClipboardList className="h-6 w-6 text-primary" />}
      tableTitle="Training Needs"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingNeedForm id={id} setId={setId} />}
      list={<TrainingNeedList editHandler={editHandler} />}
    />
  );
}

export default TrainingNeed;
