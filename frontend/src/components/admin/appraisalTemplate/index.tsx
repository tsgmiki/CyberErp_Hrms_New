import { lazy, memo } from "react";
import { ClipboardType } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AppraisalTemplateForm = memo(lazy(() => import("./form")));
const AppraisalTemplateList = memo(lazy(() => import("./list")));

function AppraisalTemplate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Appraisal Templates"
      headerDescription="Goals / competencies weight split for appraisal forms"
      headerIcon={<ClipboardType className="h-6 w-6 text-primary" />}
      tableTitle="Appraisal Templates"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AppraisalTemplateForm id={id} setId={setId} />}
      list={<AppraisalTemplateList editHandler={editHandler} />}
    />
  );
}

export default AppraisalTemplate;
