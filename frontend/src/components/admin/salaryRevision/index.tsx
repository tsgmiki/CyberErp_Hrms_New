import { lazy, memo, useState } from "react";
import { TrendingUp } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const SalaryRevisionList = memo(lazy(() => import("./list")));
const SalaryRevisionForm = memo(lazy(() => import("./form")));
const SalaryRevisionDetailModal = memo(lazy(() => import("./detailModal")));

/** HC228 — salary revision planning + scenario simulation. */
function SalaryRevision() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Salary Revisions"
        headerDescription="Plan merit/market/COLA adjustments, simulate the cost, and apply on approval"
        headerIcon={<TrendingUp className="h-6 w-6 text-primary" />}
        tableTitle="Salary Revisions"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<SalaryRevisionForm onDone={backHandler} />}
        list={<SalaryRevisionList onSelect={setSelId} />}
      />
      {selId && <SalaryRevisionDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default SalaryRevision;
