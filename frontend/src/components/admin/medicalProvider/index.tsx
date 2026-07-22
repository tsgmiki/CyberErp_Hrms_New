import { lazy, memo } from "react";
import { Stethoscope } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MedicalProviderForm = memo(lazy(() => import("./form")));
const MedicalProviderList = memo(lazy(() => import("./list")));

function MedicalProvider() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Medical Providers"
      headerDescription="Approved hospitals, clinics, laboratories and pharmacies"
      headerIcon={<Stethoscope className="h-6 w-6 text-primary" />}
      tableTitle="Medical Providers"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<MedicalProviderForm id={id} setId={setId} />}
      list={<MedicalProviderList editHandler={editHandler} />}
    />
  );
}

export default MedicalProvider;
