import { lazy, memo } from "react";
import { ThumbsUp } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RewardNominationForm = memo(lazy(() => import("./form")));
const RewardNominationList = memo(lazy(() => import("./list")));

function RewardNomination() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Award Nominations"
      headerDescription="Nominate employees for awards — approvals route through the workflow"
      headerIcon={<ThumbsUp className="h-6 w-6 text-primary" />}
      tableTitle="Award Nominations"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RewardNominationForm id={id} setId={setId} />}
      list={<RewardNominationList editHandler={editHandler} />}
    />
  );
}

export default RewardNomination;
