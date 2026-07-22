import { lazy, memo } from "react";
import { Package } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const CompanyAssetForm = memo(lazy(() => import("./form")));
const CompanyAssetList = memo(lazy(() => import("./list")));

function CompanyAsset() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Company Assets"
      headerDescription="IT equipment, access cards, keys, vehicles and tools — assignment and exit recovery"
      headerIcon={<Package className="h-6 w-6 text-primary" />}
      tableTitle="Company Assets"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<CompanyAssetForm id={id} setId={setId} />}
      list={<CompanyAssetList editHandler={editHandler} />}
    />
  );
}

export default CompanyAsset;
