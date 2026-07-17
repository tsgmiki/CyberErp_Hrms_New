"use client";
import { lazy, memo, Suspense } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import { useEntityCrudModule } from "@/template";
import getEmployee from "@/services/admin/employee/get";
import Loading from "../../common/loader/loader";

// The Employee-module leave form is strictly Annual Leave: no leave-type field, and a required
// Annual Leave Ledger (fiscal-year entitlement) selection that drives which ledger is debited.
const AnnualLeaveForm = memo(lazy(() => import("../annualLeave/form")));
const AnnualLeaveList = memo(lazy(() => import("../annualLeave/list")));

/**
 * Employee-profile "Leave Requests" tab: an HR Admin sees all of this employee's past & pending
 * annual-leave requests and can initiate a new one (the employee is fixed to the profile in view;
 * the HR admin picks which fiscal year's ledger to deduct from).
 */
function LeaveRequestSection({ employeeId }: { employeeId: string }) {
  const { t } = useTranslation();
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  const { data: employee } = useQuery({
    queryKey: ["employee", employeeId],
    queryFn: () => getEmployee(employeeId),
    enabled: !!employeeId,
  });
  const employeeName = employee?.fullName || employee?.employeeNumber || "";

  // A blank id while the form is open means "new"; otherwise we're viewing a specific request.
  const creating = showForm && !id;

  return (
    <div className="m-1 rounded-lg border border-border bg-card">
      <div className="flex items-center justify-between border-b border-border px-4 py-2.5">
        <h3 className="text-sm font-semibold text-foreground">
          {showForm
            ? creating
              ? t("New Annual Leave Request")
              : t("Annual Leave Request")
            : t("Annual Leave")}
        </h3>
        {showForm ? (
          <button
            type="button"
            onClick={backHandler}
            className="flex items-center gap-1 rounded border border-border px-3 py-1.5 text-xs font-medium text-foreground hover:border-primary hover:text-primary"
          >
            <ArrowLeft className="h-3.5 w-3.5" /> {t("Back to list")}
          </button>
        ) : (
          <button
            type="button"
            onClick={addHandler}
            className="flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus className="h-3.5 w-3.5" /> {t("New Annual Leave Request")}
          </button>
        )}
      </div>

      <div className="p-3">
        <Suspense fallback={<Loading />}>
          {showForm ? (
            <AnnualLeaveForm
              id={id}
              // Returning to the list on success (form calls setId("")).
              setId={(v) => {
                setId(v);
                if (!v) backHandler();
              }}
              lockedEmployeeId={employeeId}
              lockedEmployeeName={employeeName}
            />
          ) : (
            <AnnualLeaveList employeeId={employeeId} editHandler={editHandler} />
          )}
        </Suspense>
      </div>
    </div>
  );
}

export default LeaveRequestSection;
