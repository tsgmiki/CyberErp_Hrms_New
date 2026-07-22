import { lazy, memo } from "react";
import { CalendarRange } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const RecognitionProgramForm = memo(lazy(() => import("./form")));
const RecognitionProgramList = memo(lazy(() => import("./list")));

function RecognitionProgram() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Recognition Programs"
      headerDescription="Recurring programs such as Employee of the Month"
      headerIcon={<CalendarRange className="h-6 w-6 text-primary" />}
      tableTitle="Recognition Programs"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<RecognitionProgramForm id={id} setId={setId} />}
      list={<RecognitionProgramList editHandler={editHandler} />}
    />
  );
}

export default RecognitionProgram;
