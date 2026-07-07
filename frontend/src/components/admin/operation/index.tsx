import { lazy, memo } from "react";
import { Shield } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const OperationForm = memo(lazy(() => import("./operationForm")));
const OperationList = memo(lazy(() => import("./operationList")));

function Operation() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Operations"
      headerDescription="Manage operations and sidebar route links"
      headerIcon={<Shield className="h-6 w-6 text-primary" />}
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      list={<OperationList editHandler={editHandler} />}
      form={
        <OperationForm
          id={id}
          setOperationId={setId}
          open={showForm}
          onClose={backHandler}
        />
      }
    />
  );
}

export default Operation;
