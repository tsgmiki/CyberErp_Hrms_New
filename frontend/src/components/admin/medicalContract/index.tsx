import { lazy, memo } from "react";
import { FileSignature } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MedicalContractForm = memo(lazy(() => import("./form")));
const MedicalContractList = memo(lazy(() => import("./list")));

function MedicalContract() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Service Contracts"
      headerDescription="Contractual agreements with medical providers"
      headerIcon={<FileSignature className="h-6 w-6 text-primary" />}
      tableTitle="Service Contracts"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<MedicalContractForm id={id} setId={setId} />}
      list={<MedicalContractList editHandler={editHandler} />}
    />
  );
}

export default MedicalContract;
