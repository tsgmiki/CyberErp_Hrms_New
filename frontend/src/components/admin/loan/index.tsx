import { lazy, memo, useState } from "react";
import { HandCoins } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const LoanList = memo(lazy(() => import("./list")));
const LoanDetailModal = memo(lazy(() => import("./detailModal")));

/** HC252–259 — HR loan register: endorse, disburse and track staff-loan repayment. */
function Loan() {
  const { showForm, backHandler, addHandler } = useEntityCrudModule();
  const [selId, setSelId] = useState<string | null>(null);

  return (
    <>
      <EntityModuleShell
        title="Employee Loans"
        headerDescription="Endorse, disburse and track staff-loan repayment"
        headerIcon={<HandCoins className="h-6 w-6 text-primary" />}
        tableTitle="Employee Loans"
        hideAdd
        showForm={showForm}
        onList={backHandler}
        onAdd={addHandler}
        list={<LoanList onSelect={setSelId} />}
      />
      {selId && <LoanDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default Loan;
