import { lazy, memo, useState } from "react";
import { HeartPulse } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MyMedicalClaimForm = memo(lazy(() => import("./form")));
const MyMedicalClaimList = memo(lazy(() => import("./list")));
const MyMedicalClaimDetailModal = memo(lazy(() => import("./detailModal")));

/** HC240 — the signed-in employee submits and tracks their own medical expense claims. */
function MyMedicalClaims() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="My Medical Claims"
        headerDescription="Submit and track your medical expense claims"
        headerIcon={<HeartPulse className="h-6 w-6 text-primary" />}
        tableTitle="My Medical Claims"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<MyMedicalClaimForm onDone={backHandler} />}
        list={<MyMedicalClaimList onSelect={setSelId} />}
      />
      {selId && <MyMedicalClaimDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default MyMedicalClaims;
