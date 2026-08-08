import { lazy, memo } from "react";
import { CalendarHeart, UserX } from "lucide-react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { EntityModuleShell, useEntityRouteModule } from "@/template";
import getMyEmployee from "@/services/admin/employee/me";
import Loading from "../../common/loader/loader";

const OtherLeaveForm = memo(lazy(() => import("./form")));
const OtherLeaveList = memo(lazy(() => import("./list")));

/** Shown when the signed-in account has no employee link. */
function NotLinked() {
  const { t } = useTranslation();
  return (
    <div className="m-6 flex flex-col items-center gap-3 rounded-lg border border-dashed border-border p-8 text-center">
      <UserX className="h-10 w-10 text-muted" />
      <p className="text-sm font-medium text-foreground">{t("Your account is not linked to an employee")}</p>
      <p className="max-w-md text-xs text-muted">
        {t("Other leave is requested for your own employee record. Ask an administrator to link your user account to your employee profile.")}
      </p>
    </div>
  );
}

/** Other (non-annual) leave — static, position-based entitlements; same approvals as Annual Leave. */
function OtherLeave() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/otherLeave");

  // Same self-lock as Annual Leave: requests are always for the signed-in employee.
  const { data: me, isLoading: meLoading } = useQuery({
    queryKey: ["myEmployee"],
    queryFn: getMyEmployee,
    staleTime: 5 * 60 * 1000,
  });

  const viewing = typeof id !== "undefined" && id !== "";

  const form = meLoading ? (
    <Loading />
  ) : me || viewing ? (
    <OtherLeaveForm
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
      title="Other Leave"
      headerDescription="Maternity, paternity, mourning and similar leaves — static entitlements that never touch the annual-leave balance"
      headerIcon={<CalendarHeart className="h-6 w-6 text-primary" />}
      tableTitle="Other Leave Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={form}
      list={<OtherLeaveList editHandler={editHandler} />}
    />
  );
}
export default OtherLeave;
