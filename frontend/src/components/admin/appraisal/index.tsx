import { lazy, memo } from "react";
import { ClipboardCheck } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AppraisalForm = memo(lazy(() => import("./form")));
const AppraisalList = memo(lazy(() => import("./list")));

function Appraisal() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Appraisals"
      headerDescription="Generate, score, and finalize employee appraisals"
      headerIcon={<ClipboardCheck className="h-6 w-6 text-primary" />}
      tableTitle="Appraisals"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AppraisalForm id={id} setId={setId} />}
      list={<AppraisalList editHandler={editHandler} />}
    />
  );
}

export default Appraisal;
