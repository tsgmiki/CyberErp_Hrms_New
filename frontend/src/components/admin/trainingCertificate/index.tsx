import { lazy, memo } from "react";
import { ScrollText } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TrainingCertificateForm = memo(lazy(() => import("./form")));
const TrainingCertificateList = memo(lazy(() => import("./list")));

function TrainingCertificate() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Certifications"
      headerDescription="Issued and external certificates — expiry and renewal tracking"
      headerIcon={<ScrollText className="h-6 w-6 text-primary" />}
      tableTitle="Certifications"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TrainingCertificateForm id={id} setId={setId} />}
      list={<TrainingCertificateList editHandler={editHandler} />}
    />
  );
}

export default TrainingCertificate;
