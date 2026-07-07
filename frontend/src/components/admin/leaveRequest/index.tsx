import { lazy, memo } from "react";
import { CalendarCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const LeaveRequestForm = memo(lazy(() => import("./form")));
const LeaveRequestList = memo(lazy(() => import("./list")));

function LeaveRequest() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Leave Requests"
      headerDescription="Submit and track employee leave; approvals run through the workflow engine"
      headerIcon={<CalendarCheck className="h-6 w-6 text-primary" />}
      tableTitle="Leave Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<LeaveRequestForm id={id} setId={setId} />}
      list={<LeaveRequestList editHandler={editHandler} />}
    />
  );
}

export default LeaveRequest;
