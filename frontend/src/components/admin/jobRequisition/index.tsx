import { lazy, memo } from "react";
import { Megaphone } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const JobRequisitionForm = memo(lazy(() => import("./form")));
const JobRequisitionList = memo(lazy(() => import("./list")));

function JobRequisition() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Job Requisitions"
      headerDescription="Approvable vacancies raised from approved hiring needs; posted to the internal/external market (HC084–HC091)"
      headerIcon={<Megaphone className="h-6 w-6 text-primary" />}
      tableTitle="Job Requisitions"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<JobRequisitionForm id={id} setId={setId} />}
      list={<JobRequisitionList editHandler={editHandler} />}
    />
  );
}

export default JobRequisition;
