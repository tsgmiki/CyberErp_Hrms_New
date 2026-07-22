import { lazy, memo } from "react";
import { ShieldAlert } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const DisciplinaryCaseForm = memo(lazy(() => import("./form")));
const DisciplinaryCaseList = memo(lazy(() => import("./list")));

/**
 * Disciplinary Case Management (§3.9.3, HC222–HC225): work-unit intake (managers raise cases for
 * their subtree, HR for anyone), a measure lifetime (ValidUntil) with opt-in promotion/reward
 * blocking flags, workflow approval, and eligibility that feeds the reward/promotion modules.
 */
function DisciplinaryCase() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Disciplinary Cases"
      headerDescription="Raise and track disciplinary measures — routed for approval, with promotion/reward impact"
      headerIcon={<ShieldAlert className="h-6 w-6 text-primary" />}
      tableTitle="Disciplinary Cases"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<DisciplinaryCaseForm id={id} setId={setId} />}
      list={<DisciplinaryCaseList editHandler={editHandler} />}
    />
  );
}

export default DisciplinaryCase;
