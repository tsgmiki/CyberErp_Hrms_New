import { lazy, memo } from "react";
import { Sparkles } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CompetencyForm = memo(lazy(() => import("./form")));
const CompetencyList = memo(lazy(() => import("./list")));

function Competency() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Competencies"
      headerDescription="Manage the competency library"
      headerIcon={<Sparkles className="h-6 w-6 text-primary" />}
      tableTitle="Competencies"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CompetencyForm id={id} setId={setId} />}
      list={<CompetencyList editHandler={editHandler} />}
    />
  );
}

export default Competency;
