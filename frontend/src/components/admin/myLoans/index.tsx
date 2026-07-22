import { lazy, memo, useState } from "react";
import { HandCoins } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const MyLoanRequestForm = memo(lazy(() => import("./form")));
const MyLoanList = memo(lazy(() => import("./list")));
const MyLoanDetailModal = memo(lazy(() => import("./detailModal")));

/** HC252/HC257 — the signed-in employee requests staff loans, tracks balances and gives consent. */
function MyLoans() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="My Loans"
        headerDescription="Request a staff loan, track your balance and give service-commitment consent"
        headerIcon={<HandCoins className="h-6 w-6 text-primary" />}
        tableTitle="My Loans"
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        form={<MyLoanRequestForm onDone={backHandler} />}
        list={<MyLoanList onSelect={setSelId} />}
      />
      {selId && <MyLoanDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default MyLoans;
