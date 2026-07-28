"use client";
import { lazy, memo, Suspense } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { Plus, ArrowLeft } from "lucide-react";
import { useEntityCrudModule } from "@/template";
import getEmployee from "@/services/admin/employee/get";
import Loading from "../../common/loader/loader";

// Other (non-annual) leave: static, position-based entitlements (maternity/paternity/mourning…)
// that never touch the annual-leave ledger — same approval mechanism as Annual Leave.
const OtherLeaveForm = memo(lazy(() => import("../otherLeave/form")));
const OtherLeaveList = memo(lazy(() => import("../otherLeave/list")));

/**
 * Employee-profile "Other Leave" tab: all of this employee's other-leave requests, with the
 * ability to initiate a new one (the employee is fixed to the profile in view; the entitlement
 * dropdown offers only the active fiscal year's settings their gender qualifies for).
 */
function OtherLeaveSection({ employeeId }: { employeeId: string }) {
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
              ? t("New Other Leave Request")
              : t("Other Leave Request")
            : t("Other Leave")}
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
            <Plus className="h-3.5 w-3.5" /> {t("New Other Leave Request")}
          </button>
        )}
      </div>

      <div className="p-3">
        <Suspense fallback={<Loading />}>
          {showForm ? (
            <OtherLeaveForm
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
            <OtherLeaveList employeeId={employeeId} editHandler={editHandler} />
          )}
        </Suspense>
      </div>
    </div>
  );
}

export default OtherLeaveSection;
