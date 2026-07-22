import { lazy, memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Receipt, FileText, BarChart3 } from "lucide-react";
import { EntityModuleShell } from "@/template";

const MedicalClaimList = memo(lazy(() => import("./list")));
const MedicalExpenseReport = memo(lazy(() => import("./expenseReport")));
const MedicalClaimDetailModal = memo(lazy(() => import("./detailModal")));

/** HC239–246 — HR medical claim register: approve/reject/reimburse + expense report. */
function MedicalClaim() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<"claims" | "report">("claims");
  const [selId, setSelId] = useState<string | null>(null);

  const tabButton = (key: "claims" | "report", label: string, icon: React.ReactNode) => (
    <button
      type="button"
      onClick={() => setTab(key)}
      className={`inline-flex items-center gap-1.5 rounded px-3 py-1 text-sm ${tab === key ? "bg-primary text-on-accent" : "text-muted"}`}
    >
      {icon} {t(label)}
    </button>
  );

  return (
    <>
      <EntityModuleShell
        title="Medical Claims"
        headerDescription="Review, approve, reject and reimburse medical claims"
        headerIcon={<Receipt className="h-6 w-6 text-primary" />}
        tableTitle="Medical Claims"
        hideAdd
        hideBack
        showForm={false}
        onList={() => undefined}
        onAdd={() => undefined}
      >
        <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
          <div className="flex w-fit rounded-md border border-border p-0.5">
            {tabButton("claims", "Claims", <FileText size={14} />)}
            {tabButton("report", "Expense Report", <BarChart3 size={14} />)}
          </div>
          {tab === "claims" ? <MedicalClaimList onSelect={setSelId} /> : <MedicalExpenseReport />}
        </div>
      </EntityModuleShell>
      {selId && <MedicalClaimDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default MedicalClaim;
