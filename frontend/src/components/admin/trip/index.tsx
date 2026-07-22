import { lazy, memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { Plane, FileText, Clock } from "lucide-react";
import { EntityModuleShell } from "@/template";

const TripList = memo(lazy(() => import("./list")));
const TripAging = memo(lazy(() => import("./aging")));
const TripDetailModal = memo(lazy(() => import("./detailModal")));

/** HC260–268 — HR trip register: approve/pay-advance/settle + expenses + the advance aging report. */
function Trip() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<"trips" | "aging">("trips");
  const [selId, setSelId] = useState<string | null>(null);

  const tabButton = (key: "trips" | "aging", label: string, icon: React.ReactNode) => (
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
        title="Business Trips"
        headerDescription="Approve, pay advances, settle and track outstanding travel advances"
        headerIcon={<Plane className="h-6 w-6 text-primary" />}
        tableTitle="Business Trips"
        hideAdd
        hideBack
        showForm={false}
        onList={() => undefined}
        onAdd={() => undefined}
      >
        <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
          <div className="flex w-fit rounded-md border border-border p-0.5">
            {tabButton("trips", "Trips", <FileText size={14} />)}
            {tabButton("aging", "Advance Aging", <Clock size={14} />)}
          </div>
          {tab === "trips" ? <TripList onSelect={setSelId} /> : <TripAging onSelect={setSelId} />}
        </div>
      </EntityModuleShell>
      {selId && <TripDetailModal id={selId} onClose={() => setSelId(null)} />}
    </>
  );
}

export default Trip;
