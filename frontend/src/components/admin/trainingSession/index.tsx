import { lazy, memo } from "react";
import { CalendarDays } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingSessionForm = memo(lazy(() => import("./form")));
const TrainingSessionList = memo(lazy(() => import("./list")));

function TrainingSession() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Training Sessions"
      headerDescription="Schedule deliveries, track participation and costs"
      headerIcon={<CalendarDays className="h-6 w-6 text-primary" />}
      tableTitle="Training Sessions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingSessionForm id={id} setId={setId} />}
      list={<TrainingSessionList editHandler={editHandler} />}
    />
  );
}

export default TrainingSession;
