import { lazy, memo } from "react";
import { Tags } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingCategoryForm = memo(lazy(() => import("./form")));
const TrainingCategoryList = memo(lazy(() => import("./list")));

function TrainingCategory() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Training Categories"
      headerDescription="Categorize courses — technical, leadership, compliance, education programs"
      headerIcon={<Tags className="h-6 w-6 text-primary" />}
      tableTitle="Training Categories"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingCategoryForm id={id} setId={setId} />}
      list={<TrainingCategoryList editHandler={editHandler} />}
    />
  );
}

export default TrainingCategory;
