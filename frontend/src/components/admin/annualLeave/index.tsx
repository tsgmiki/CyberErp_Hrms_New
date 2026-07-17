import { lazy, memo } from "react";
import { CalendarCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AnnualLeaveForm = memo(lazy(() => import("./form")));
const AnnualLeaveList = memo(lazy(() => import("./list")));

function AnnualLeave() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Annual Leave"
      headerDescription="Submit and track annual-leave requests; each is charged against the employee's annual-leave ledger"
      headerIcon={<CalendarCheck className="h-6 w-6 text-primary" />}
      tableTitle="Annual Leave Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AnnualLeaveForm id={id} setId={setId} />}
      list={<AnnualLeaveList editHandler={editHandler} />}
    />
  );
}

export default AnnualLeave;
