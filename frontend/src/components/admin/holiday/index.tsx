import { lazy, memo } from "react";
import { CalendarClock } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const HolidayForm = memo(lazy(() => import("./form")));
const HolidayList = memo(lazy(() => import("./list")));

function Holiday() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/holiday");

  return (
    <EntityModuleShell
      title="Holidays"
      headerDescription="Public, religious and organizational holidays used in working-day calculations"
      headerIcon={<CalendarClock className="h-6 w-6 text-primary" />}
      tableTitle="Holiday Calendar"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<HolidayForm id={id} setId={setId} />}
      list={<HolidayList editHandler={editHandler} />}
    />
  );
}

export default Holiday;
