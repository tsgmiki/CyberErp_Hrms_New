import { lazy, memo, useState } from "react";
import { ShieldCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";
import ScheduleModal from "./scheduleModal";

const InsurancePolicyForm = memo(lazy(() => import("./form")));
const InsurancePolicyList = memo(lazy(() => import("./list")));

function InsurancePolicy() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();
  const [scheduleId, setScheduleId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Insurance Policies"
        headerDescription="Group insurance policies, premium schedules and renewals"
        headerIcon={<ShieldCheck className="h-6 w-6 text-primary" />}
        tableTitle="Insurance Policies"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<InsurancePolicyForm id={id} setId={setId} />}
        list={<InsurancePolicyList editHandler={editHandler} scheduleHandler={setScheduleId} />}
      />
      {scheduleId && <ScheduleModal policyId={scheduleId} onClose={() => setScheduleId(null)} />}
    </>
  );
}

export default InsurancePolicy;
