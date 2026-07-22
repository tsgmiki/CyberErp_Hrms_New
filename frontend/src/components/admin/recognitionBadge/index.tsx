import { lazy, memo } from "react";
import { Award } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RecognitionBadgeForm = memo(lazy(() => import("./form")));
const RecognitionBadgeList = memo(lazy(() => import("./list")));

function RecognitionBadge() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Recognition Badges"
      headerDescription="Configure awards and badges for recognition"
      headerIcon={<Award className="h-6 w-6 text-primary" />}
      tableTitle="Recognition Badges"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RecognitionBadgeForm id={id} setId={setId} />}
      list={<RecognitionBadgeList editHandler={editHandler} />}
    />
  );
}

export default RecognitionBadge;
