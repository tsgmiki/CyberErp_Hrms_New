import { lazy, memo } from "react";
import { FileBarChart } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const ReportDefinitionForm = memo(lazy(() => import("./form")));
const ReportDefinitionList = memo(lazy(() => import("./list")));

function ReportDefinition() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Report Definitions"
      headerDescription="Register stored-procedure-driven reports: each report names its own procedure and declares its input parameters"
      headerIcon={<FileBarChart className="h-6 w-6 text-primary" />}
      tableTitle="Report Definitions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<ReportDefinitionForm id={id} setId={setId} />}
      list={<ReportDefinitionList editHandler={editHandler} />}
    />
  );
}

export default ReportDefinition;
