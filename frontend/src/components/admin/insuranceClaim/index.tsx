import { lazy, memo, useState } from "react";
import { FileHeart } from "lucide-react";
import { EntityModuleShell } from "@/template";

const InsuranceClaimList = memo(lazy(() => import("./list")));
const InsuranceClaimDetailModal = memo(lazy(() => import("./detailModal")));

/** HC248/HC249 — HR insurance claim register: review, approve, reject and reimburse. */
function InsuranceClaim() {
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Insurance Claims"
        headerDescription="Review, approve, reject and reimburse insurance coverage claims"
        headerIcon={<FileHeart className="h-6 w-6 text-primary" />}
        tableTitle="Insurance Claims"
        hideAdd
        showForm={false}
        onList={() => undefined}
        onAdd={() => undefined}
        list={<InsuranceClaimList onSelect={setSelId} />}
      />
      {selId && <InsuranceClaimDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default InsuranceClaim;
