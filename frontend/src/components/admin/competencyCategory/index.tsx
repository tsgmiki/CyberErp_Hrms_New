import { lazy, memo } from "react";
import { Shapes } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CompetencyCategoryForm = memo(lazy(() => import("./form")));
const CompetencyCategoryList = memo(lazy(() => import("./list")));

function CompetencyCategory() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Competency Categories"
      headerDescription="Manage competency categories"
      headerIcon={<Shapes className="h-6 w-6 text-primary" />}
      tableTitle="Competency Categories"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CompetencyCategoryForm id={id} setId={setId} />}
      list={<CompetencyCategoryList editHandler={editHandler} />}
    />
  );
}

export default CompetencyCategory;
