import { lazy, memo } from "react";
import { Gavel } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AppraisalAppealForm = memo(lazy(() => import("./form")));
const AppraisalAppealList = memo(lazy(() => import("./list")));

function AppraisalAppeal() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Appeals"
      headerDescription="Review and resolve employee appraisal appeals"
      headerIcon={<Gavel className="h-6 w-6 text-primary" />}
      tableTitle="Appeals"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      hideAdd
      form={<AppraisalAppealForm id={id} setId={setId} />}
      list={<AppraisalAppealList editHandler={editHandler} />}
    />
  );
}

export default AppraisalAppeal;
