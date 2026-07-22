import { lazy, memo } from "react";
import { CalendarCog } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const WorkWeekConfigurationForm = memo(lazy(() => import("./form")));
const WorkWeekConfigurationList = memo(lazy(() => import("./list")));

function WorkWeekConfiguration() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Work Week Configuration"
      headerDescription="Define which weekdays are full, half or rest days; the active configuration drives leave & attendance day counts"
      headerIcon={<CalendarCog className="h-6 w-6 text-primary" />}
      tableTitle="Work Week Configurations"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<WorkWeekConfigurationForm id={id} setId={setId} />}
      list={<WorkWeekConfigurationList editHandler={editHandler} />}
    />
  );
}

export default WorkWeekConfiguration;
