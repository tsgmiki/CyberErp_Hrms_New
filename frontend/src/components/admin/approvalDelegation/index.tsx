import { lazy, memo } from "react";
import { UserCheck } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const ApprovalDelegationForm = memo(lazy(() => import("./form")));
const ApprovalDelegationList = memo(lazy(() => import("./list")));

/**
 * Approval delegation — HR's view of every stand-in arrangement in the tenant.
 *
 * <p>An approver's own delegations live on the self-service "My Delegations" screen; this one is
 * the administrative register, and the only place the tenant-wide eligibility policy is edited.</p>
 */
function ApprovalDelegation() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/approvalDelegation");

  return (
    <EntityModuleShell
      title="Approval Delegation"
      headerDescription="Lend an approver's authority to a stand-in for a bounded period — scoped by process, capped by amount, and fully audited"
      headerIcon={<UserCheck className="h-6 w-6 text-primary" />}
      tableTitle="Delegations"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ApprovalDelegationForm id={id} setId={setId} />}
      list={<ApprovalDelegationList editHandler={editHandler} />}
    />
  );
}

export default ApprovalDelegation;
