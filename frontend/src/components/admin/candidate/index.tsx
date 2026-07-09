import { lazy, memo } from "react";
import { Users } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CandidateForm = memo(lazy(() => import("./form")));
const CandidateList = memo(lazy(() => import("./list")));

function Candidate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Candidates"
      headerDescription="Centralized applicant database with consent, resumes, talent pool and internal matching (HC089–HC097)"
      headerIcon={<Users className="h-6 w-6 text-primary" />}
      tableTitle="Candidates"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CandidateForm id={id} setId={setId} />}
      list={<CandidateList editHandler={editHandler} />}
    />
  );
}

export default Candidate;
