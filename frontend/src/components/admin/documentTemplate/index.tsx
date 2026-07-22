import { lazy, memo } from "react";
import { FileText } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const DocumentTemplateForm = memo(lazy(() => import("./form")));
const DocumentTemplateList = memo(lazy(() => import("./list")));

function DocumentTemplate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Document Templates"
      headerDescription="Configure reusable letter & ID-card templates, then print them per employee"
      headerIcon={<FileText className="h-6 w-6 text-primary" />}
      tableTitle="Document Templates"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<DocumentTemplateForm id={id} setId={setId} />}
      list={<DocumentTemplateList editHandler={editHandler} />}
    />
  );
}

export default DocumentTemplate;
