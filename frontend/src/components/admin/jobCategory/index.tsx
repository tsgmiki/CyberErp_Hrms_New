import { lazy, memo } from "react";
import { Tags } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const JobCategoryForm = memo(lazy(() => import("./form")));
const JobCategoryList = memo(lazy(() => import("./list")));

function JobCategory() {
  // URL-backed: /jobCategory (list) · /jobCategory/new · /jobCategory/{guid}.
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/jobCategory");

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
