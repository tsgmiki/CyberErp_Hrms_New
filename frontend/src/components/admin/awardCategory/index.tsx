import { lazy, memo } from "react";
import { Tags } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const AwardCategoryForm = memo(lazy(() => import("./form")));
const AwardCategoryList = memo(lazy(() => import("./list")));

function AwardCategory() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/awardCategory");

  return (
    <EntityModuleShell
      title="Award Categories"
      headerDescription="Group awards under shared eligibility criteria"
      headerIcon={<Tags className="h-6 w-6 text-primary" />}
      tableTitle="Award Categories"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AwardCategoryForm id={id} setId={setId} />}
      list={<AwardCategoryList editHandler={editHandler} />}
    />
  );
}

export default AwardCategory;
