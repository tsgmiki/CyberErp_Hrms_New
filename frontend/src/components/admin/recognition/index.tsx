import { lazy, memo } from "react";
import { Sparkles } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RecognitionForm = memo(lazy(() => import("./form")));
const RecognitionList = memo(lazy(() => import("./list")));

function Recognition() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Recognition"
      headerDescription="Recognize high performers with awards and badges"
      headerIcon={<Sparkles className="h-6 w-6 text-primary" />}
      tableTitle="Recognition Board"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RecognitionForm id={id} setId={setId} />}
      list={<RecognitionList editHandler={editHandler} />}
    />
  );
}

export default Recognition;
