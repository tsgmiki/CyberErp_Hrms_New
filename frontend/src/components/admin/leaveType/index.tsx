import { lazy, memo } from "react";
import { CalendarDays } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const LeaveTypeForm = memo(lazy(() => import("./form")));
const LeaveTypeList = memo(lazy(() => import("./list")));

function LeaveType() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/leaveType");

  return (
    <EntityModuleShell
      title="Leave Types"
      headerDescription="Configure leave categories, entitlements and policy rules"
      headerIcon={<CalendarDays className="h-6 w-6 text-primary" />}
      tableTitle="Leave Types"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<LeaveTypeForm id={id} setId={setId} />}
      list={<LeaveTypeList editHandler={editHandler} />}
    />
  );
}

export default LeaveType;
