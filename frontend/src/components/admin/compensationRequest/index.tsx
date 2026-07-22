import { lazy, memo, useState } from "react";
import { MessageSquareWarning } from "lucide-react";
import { EntityModuleShell } from "@/template";
import type { CompensationRequestModel } from "@/models";

const CompensationRequestList = memo(lazy(() => import("./list")));
const CompensationRequestDetailModal = memo(lazy(() => import("./detailModal")));

/** HC234 — compensation requests: benefit-change / payroll-discrepancy review + resolution. */
function CompensationRequest() {
  const [selRec, setSelRec] = useState<CompensationRequestModel | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Compensation Requests"
        headerDescription="Review and resolve benefit-change and payroll-discrepancy requests"
        headerIcon={<MessageSquareWarning className="h-6 w-6 text-primary" />}
        tableTitle="Compensation Requests"
        hideAdd
        hideBack
        showForm={false}
        onList={() => undefined}
        onAdd={() => undefined}
        list={<CompensationRequestList onSelect={setSelRec} />}
      />
      {selRec && <CompensationRequestDetailModal record={selRec} onClose={() => setSelRec(null)} />}
    </>
  );
}

export default memo(CompensationRequest);
