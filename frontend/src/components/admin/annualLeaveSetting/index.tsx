import { lazy, memo } from "react";
import { SlidersHorizontal } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AnnualLeaveSettingForm = memo(lazy(() => import("./form")));
const AnnualLeaveSettingList = memo(lazy(() => import("./list")));

function AnnualLeaveSetting() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Annual Leave Settings"
      headerDescription="Per-fiscal-year accrual policy: entitlements by service length, caps, carry-forward expiry"
      headerIcon={<SlidersHorizontal className="h-6 w-6 text-primary" />}
      tableTitle="Annual Leave Settings"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AnnualLeaveSettingForm id={id} setId={setId} />}
      list={<AnnualLeaveSettingList editHandler={editHandler} />}
    />
  );
}

export default AnnualLeaveSetting;
