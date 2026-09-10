import { lazy, memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { ShieldCheck, ListTodo, LayoutDashboard, TrendingUp, FileSignature } from "lucide-react";
import { EntityModuleShell, useEntityCrudModule } from "@/template";

const LearningAssignmentForm = memo(lazy(() => import("./form")));
const LearningAssignmentList = memo(lazy(() => import("./list")));
const ComplianceDashboard = memo(lazy(() => import("./dashboard")));
const ObligationsPanel = memo(lazy(() => import("./obligations")));
const EffectivenessPanel = memo(lazy(() => import("./effectiveness")));
const RecordsPanel = memo(lazy(() => import("./records")));

type Tab = "rules" | "dashboard" | "obligations" | "records" | "effectiveness";

const TABS: { id: Tab; name: string; icon: typeof ShieldCheck }[] = [
  { id: "dashboard", name: "Compliance", icon: LayoutDashboard },
  { id: "rules", name: "Assignments", icon: ShieldCheck },
  { id: "obligations", name: "Who Owes What", icon: ListTodo },
  { id: "records", name: "Signed Records", icon: FileSignature },
  { id: "effectiveness", name: "Effectiveness", icon: TrendingUp },
];

/**
 * Mandatory training: the rules, the standing, the individual records, and whether any of it works.
 *
 * <p>Four tabs rather than four menu entries, because they are four views of one subject and nobody
 * navigates to "who owes what" without first having seen the number that sent them there. The
 * dashboard leads, since that is the question this screen exists to answer.</p>
 *
 * <p>The Assignments tab uses the state-based CRUD hook rather than the route-based one: the URL
 * already belongs to the tab bar, and a nested route module would fight it for the same segment.</p>
 */
function LearningCompliance() {
  const { t } = useTranslation();
  const [tab, setTab] = useState<Tab>("dashboard");
  const { id, setId, showForm, backHandler, addHandler, editHandler } = useEntityCrudModule();

  return (
    <div className="flex h-full min-h-0 flex-col">
      <nav className="mb-3 flex flex-wrap gap-1.5 px-3 pt-3">
        {TABS.map((x) => {
          const Icon = x.icon;
          const on = tab === x.id;
          return (
            <button
              key={x.id}
              type="button"
              onClick={() => setTab(x.id)}
              className={`inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-xs font-semibold ${
                on ? "bg-primary text-on-accent" : "border border-border text-muted hover:bg-secondary"
              }`}
            >
              <Icon className="h-3.5 w-3.5" /> {t(x.name)}
            </button>
          );
        })}
      </nav>

      <div className="min-h-0 flex-1 overflow-auto">
        {tab === "rules" ? (
          <EntityModuleShell
            title="Mandatory Training"
            headerDescription="Assign a course to a population, with a deadline and a recertification cycle"
            headerIcon={<ShieldCheck className="h-6 w-6 text-primary" />}
            tableTitle="Assignments"
            showForm={showForm}
            onList={backHandler}
            onAdd={addHandler}
            form={<LearningAssignmentForm id={id} setId={setId} />}
            list={<LearningAssignmentList editHandler={editHandler} />}
          />
        ) : (
          <div className="px-3 pb-3">
            {tab === "dashboard" && <ComplianceDashboard />}
            {tab === "obligations" && <ObligationsPanel />}
            {tab === "records" && <RecordsPanel />}
            {tab === "effectiveness" && <EffectivenessPanel />}
          </div>
        )}
      </div>
    </div>
  );
}

export default LearningCompliance;
