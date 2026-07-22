import { lazy, memo, useState } from "react";
import { FileHeart } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MyInsuranceClaimForm = memo(lazy(() => import("./form")));
const MyInsuranceClaimList = memo(lazy(() => import("./list")));
const MyInsuranceClaimDetailModal = memo(lazy(() => import("./detailModal")));

/** HC248 — the signed-in employee submits and tracks their own insurance claims. */
function MyInsuranceClaims() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="My Insurance Claims"
        headerDescription="Submit and track your insurance coverage claims"
        headerIcon={<FileHeart className="h-6 w-6 text-primary" />}
        tableTitle="My Insurance Claims"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<MyInsuranceClaimForm onDone={backHandler} />}
        list={<MyInsuranceClaimList onSelect={setSelId} />}
      />
      {selId && <MyInsuranceClaimDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default MyInsuranceClaims;
