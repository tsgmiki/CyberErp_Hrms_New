import { lazy, memo } from "react";
import { Grid3x3 } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const TalentReviewForm = memo(lazy(() => import("./form")));
const TalentReviewList = memo(lazy(() => import("./list")));

function TalentReview() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/talentReview");
  return (
    <EntityModuleShell
      title="Talent Reviews"
      headerDescription="9-box talent review & calibration sessions"
      headerIcon={<Grid3x3 className="h-6 w-6 text-primary" />}
      tableTitle="Talent Reviews"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TalentReviewForm id={id} setId={setId} />}
      list={<TalentReviewList editHandler={editHandler} />}
    />
  );
}
export default TalentReview;
