import { lazy, memo } from "react";
import { Handshake } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";

const MentorshipForm = memo(lazy(() => import("./form")));
const MentorshipList = memo(lazy(() => import("./list")));

function Mentorship() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityRouteModule("/mentorship");
  return (
    <EntityModuleShell
      title="Mentorships"
      headerDescription="Pair mentors with mentees for career and succession development"
      headerIcon={<Handshake className="h-6 w-6 text-primary" />}
      tableTitle="Mentorships"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<MentorshipForm id={id} setId={setId} />}
      list={<MentorshipList editHandler={editHandler} />}
    />
  );
}

export default Mentorship;
