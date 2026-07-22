import { lazy, memo } from "react";
import { Medal } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AchievementForm = memo(lazy(() => import("./form")));
const AchievementList = memo(lazy(() => import("./list")));

function Achievement() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Achievements"
      headerDescription="Record employee achievements and milestones"
      headerIcon={<Medal className="h-6 w-6 text-primary" />}
      tableTitle="Achievements"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AchievementForm id={id} setId={setId} />}
      list={<AchievementList editHandler={editHandler} />}
    />
  );
}

export default Achievement;
