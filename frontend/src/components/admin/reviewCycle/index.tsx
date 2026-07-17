import { lazy, memo } from "react";
import { CalendarClock } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ReviewCycleForm = memo(lazy(() => import("./form")));
const ReviewCycleList = memo(lazy(() => import("./list")));

function ReviewCycle() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Review Cycles"
      headerDescription="Configure appraisal cycles and their windows"
      headerIcon={<CalendarClock className="h-6 w-6 text-primary" />}
      tableTitle="Review Cycles"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ReviewCycleForm id={id} setId={setId} />}
      list={<ReviewCycleList editHandler={editHandler} />}
    />
  );
}

export default ReviewCycle;
