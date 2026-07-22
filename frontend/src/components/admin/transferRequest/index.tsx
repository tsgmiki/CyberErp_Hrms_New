import { lazy, memo } from "react";
import { ArrowLeftRight } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const TransferRequestForm = memo(lazy(() => import("./form")));
const TransferRequestList = memo(lazy(() => import("./list")));

/**
 * Employee Transfer requests (§3.7.3, HC170–176): self-service / manager / HR initiation, workflow
 * approval, eligibility + impact assessment, formal transfer notice, and effective-date execution.
 */
function TransferRequest() {
  const { id, setId, showForm, backHandler, addHandler, editHandler } =
    useEntityCrudModule();

  return (
    <EntityModuleShell
      title="Transfer Requests"
      headerDescription="Request, assess and approve employee transfers — applied on the effective date"
      headerIcon={<ArrowLeftRight className="h-6 w-6 text-primary" />}
      tableTitle="Transfer Requests"
      showForm={showForm}
      onList={backHandler}
      onAdd={addHandler}
      form={<TransferRequestForm id={id} setId={setId} />}
      list={<TransferRequestList editHandler={editHandler} />}
    />
  );
}

export default TransferRequest;
