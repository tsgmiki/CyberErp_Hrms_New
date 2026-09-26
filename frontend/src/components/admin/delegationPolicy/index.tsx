import { lazy, memo } from "react";
import { SlidersHorizontal } from "lucide-react";
import { EntityModuleShell } from "@/template";

const DelegationPolicyForm = memo(lazy(() => import("./form")));

/**
 * Organisation-wide delegation rules — who is allowed to hold somebody else's approval authority.
 *
 * A SINGLETON, like Increment Rules: one policy per tenant, so there is no list to page through,
 * nothing to add and nothing to go back to. The shell is used anyway so the screen keeps the
 * standard header and chrome, with the list/add/back actions suppressed rather than left on screen
 * doing nothing.
 */
function DelegationPolicy() {
  return (
    <EntityModuleShell
      title="Delegation Rules"
      headerDescription="Who may hold another approver's authority: experience, salary parity, how long a delegation may run, and how much a stand-in may sign for"
      headerIcon={<SlidersHorizontal className="h-6 w-6 text-primary" />}
      showForm
      hideAdd
      hideBack
      form={<DelegationPolicyForm />}
    />
  );
}

export default DelegationPolicy;
