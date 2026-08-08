import { lazy, memo } from "react";
import { CalendarCog } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const OtherLeaveSettingForm = memo(lazy(() => import("./form")));
const OtherLeaveSettingList = memo(lazy(() => import("./list")));

/** Static per-fiscal-year policies for the NON-annual leave types (maternity, paternity, mourning…). */
function OtherLeaveSetting() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/otherLeaveSetting");
  return (
    <EntityModuleShell
      title="Other Leave Settings"
      headerDescription="Static, position-based entitlements per fiscal year — gender-aware, no accrual, never charged against annual leave"
      headerIcon={<CalendarCog className="h-6 w-6 text-primary" />}
      tableTitle="Other Leave Settings"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<OtherLeaveSettingForm id={id} setId={setId} />}
      list={<OtherLeaveSettingList editHandler={editHandler} />}
    />
  );
}
export default OtherLeaveSetting;
