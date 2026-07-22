import { lazy, memo } from "react";
import { Route } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const LearningPathForm = memo(lazy(() => import("./form")));
const LearningPathList = memo(lazy(() => import("./list")));

function LearningPath() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Learning Paths"
      headerDescription="Structured course sequences aligned with career progression"
      headerIcon={<Route className="h-6 w-6 text-primary" />}
      tableTitle="Learning Paths"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<LearningPathForm id={id} setId={setId} />}
      list={<LearningPathList editHandler={editHandler} />}
    />
  );
}

export default LearningPath;
