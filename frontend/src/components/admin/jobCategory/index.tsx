import { lazy, memo } from "react";
import { Tags } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const JobCategoryForm = memo(lazy(() => import("./form")));
const JobCategoryList = memo(lazy(() => import("./list")));

function JobCategory() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Job Categories"
      headerDescription="Manage job categories"
      headerIcon={<Tags className="h-6 w-6 text-primary" />}
      tableTitle="Job Categories"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<JobCategoryForm id={id} setId={setId} />}
      list={<JobCategoryList editHandler={editHandler} />}
    />
  );
}

export default JobCategory;
