import { lazy, memo } from "react";
import { ListChecks } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const QuestionBankForm = memo(lazy(() => import("./form")));
const QuestionBankList = memo(lazy(() => import("./list")));

function QuestionBank() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityRouteModule("/questionBank");

  return (
    <EntityModuleShell
      title="Question Banks"
      headerDescription="Reusable quiz questions — write once, import into any course quiz"
      headerIcon={<ListChecks className="h-6 w-6 text-primary" />}
      tableTitle="Question Banks"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<QuestionBankForm id={id} setId={setId} />}
      list={<QuestionBankList editHandler={editHandler} />}
    />
  );
}

export default QuestionBank;
