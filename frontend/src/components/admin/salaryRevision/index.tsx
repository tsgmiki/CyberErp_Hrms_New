import { lazy, memo } from "react";
import { TrendingUp } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const SalaryRevisionList = memo(lazy(() => import("./list")));
const SalaryRevisionForm = memo(lazy(() => import("./form")));
const SalaryRevisionDetail = memo(lazy(() => import("./detail")));

/**
 * HC228 — salary revision planning + scenario simulation.
 *
 * Three URL-backed views rather than a list plus a popup:
 *   /salaryRevision          the revisions grid
 *   /salaryRevision/new      the planning form
 *   /salaryRevision/{guid}   that revision's increment grid (was a modal)
 *
 * Selecting a row swaps the page to the increment grid; the shell's standard Back arrow returns.
 * Because the record is in the URL, a specific revision is now linkable and survives a refresh.
 */
function SalaryRevision() {
  const { id, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/salaryRevision");

  // showForm covers both /new and /{guid}; the id distinguishes them.
  const isDetail = showForm && !!id;

  return (
    <EntityModuleShell
      title="Salary Revisions"
      headerDescription="Plan merit/market/COLA adjustments, simulate the cost, and apply on approval"
      headerIcon={<TrendingUp className="h-6 w-6 text-primary" />}
      tableTitle={isDetail ? "Salary Increment" : "Salary Revisions"}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={
        isDetail
          ? <SalaryRevisionDetail id={id} onBack={backHandler} />
          : <SalaryRevisionForm onDone={backHandler} />
      }
      list={<SalaryRevisionList onSelect={editHandler} />}
    />
  );
}

export default SalaryRevision;
