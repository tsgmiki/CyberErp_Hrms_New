import { lazy, memo } from "react";
import { CalendarCheck, UserX } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import getMyEmployee from "@/services/admin/employee/me";
import Loading from "../../common/loader/loader";

const AnnualLeaveForm = memo(lazy(() => import("./form")));
const AnnualLeaveList = memo(lazy(() => import("./list")));

/** Shown when the signed-in account has no employee link — leave can't be requested for "nobody". */
function NotLinked() {
  const { t } = useTranslation();
  return (
    <div className="m-6 flex flex-col items-center gap-3 rounded-lg border border-dashed border-border p-8 text-center">
      <UserX className="h-10 w-10 text-muted" />
      <p className="text-sm font-medium text-foreground">{t("Your account is not linked to an employee")}</p>
      <p className="max-w-md text-xs text-muted">
        {t("Annual leave is requested for your own employee record. Ask an administrator to link your user account to your employee profile.")}
      </p>
    </div>
  );
}

function AnnualLeave() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  // The Employee field is ALWAYS the signed-in employee here — resolved once, then locked
  // read-only in the form (no other employee can be selected on this screen).
  const { data: me, isLoading: meLoading } = useQuery({
    queryKey: ["myEmployee"],
    queryFn: getMyEmployee,
    staleTime: 5 * 60 * 1000,
  });

  const viewing = typeof id !== "undefined" && id !== "";

  const form = meLoading ? (
    <Loading />
  ) : me || viewing ? (
    <AnnualLeaveForm
      id={id}
      setId={setId}
      lockedEmployeeId={me?.id}
      lockedEmployeeName={me ? `${me.employeeNumber ?? ""} — ${me.fullName ?? ""}` : undefined}
    />
  ) : (
    <NotLinked />
  );

  return (
    <EntityModuleShell
      title="Annual Leave"
      headerDescription="Submit and track annual-leave requests; each is charged against the employee's annual-leave ledger"
      headerIcon={<CalendarCheck className="h-6 w-6 text-primary" />}
      tableTitle="Annual Leave Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={form}
      list={<AnnualLeaveList editHandler={editHandler} />}
    />
  );
}

export default AnnualLeave;
