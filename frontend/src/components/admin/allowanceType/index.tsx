import { lazy, memo } from "react";
import { Coins } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const AllowanceTypeForm = memo(lazy(() => import("./form")));
const AllowanceTypeList = memo(lazy(() => import("./list")));

function AllowanceType() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Allowance Types"
      headerDescription="The catalogue of earnings — fixed amounts or a percent of base, taxable or exempt"
      headerIcon={<Coins className="h-6 w-6 text-primary" />}
      tableTitle="Allowance Types"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<AllowanceTypeForm id={id} setId={setId} />}
      list={<AllowanceTypeList editHandler={editHandler} />}
    />
  );
}

export default AllowanceType;
