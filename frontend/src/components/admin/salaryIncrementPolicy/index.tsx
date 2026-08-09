import { lazy, memo } from "react";
import { SlidersHorizontal } from "lucide-react";
import { EntityModuleShell } from "@/template";

const SalaryIncrementPolicyForm = memo(lazy(() => import("./form")));

/**
 * Increment eligibility rules — who qualifies for a salary increment, and for how much of it.
 *
 * A SINGLETON, unlike every other module here: one active policy per tenant, so there is no list to
 * page through, nothing to add and nothing to go back to. The shell is used anyway so the screen
 * keeps the standard header/chrome, with `showForm` pinned true and the list/add/back actions
 * suppressed rather than left on screen doing nothing.
 */
function SalaryIncrementPolicy() {
  return (
    <EntityModuleShell
      title="Increment Rules"
      headerDescription="Who qualifies for a salary increment: minimum service, first-year proration, and disciplinary exclusions"
      headerIcon={<SlidersHorizontal className="h-6 w-6 text-primary" />}
      showForm
      hideAdd
      hideBack
      form={<SalaryIncrementPolicyForm />}
    />
  );
}

export default SalaryIncrementPolicy;
