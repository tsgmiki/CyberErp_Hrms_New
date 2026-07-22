import { lazy, memo, useCallback, useState } from "react";
import { Network, GitFork, PanelsTopLeft } from "lucide-react";
import { EntityModuleShell } from "@/template";
import type { OrgUnitTreeNode } from "@/models";

const OrgTree = memo(lazy(() => import("./orgTree")));
const OrganizationUnitGrid = memo(lazy(() => import("./grid")));
const OrganizationUnitForm = memo(lazy(() => import("./form")));
const OrgChart = memo(lazy(() => import("@/components/common/orgChart/orgChart")));

type View = "structure" | "chart";

function ViewToggle({ view, setView }: { view: View; setView: (v: View) => void }) {
  const base = "flex items-center gap-1 rounded px-3 py-1 text-sm";
  const on = "bg-primary text-on-accent";
  const off = "bg-secondary text-muted hover:text-foreground";
  return (
    <div className="mb-2 flex gap-2">
      <button type="button" className={`${base} ${view === "structure" ? on : off}`} onClick={() => setView("structure")}>
        <PanelsTopLeft className="h-4 w-4" /> Tree &amp; Grid
      </button>
      <button type="button" className={`${base} ${view === "chart" ? on : off}`} onClick={() => setView("chart")}>
        <GitFork className="h-4 w-4" /> Org Chart
      </button>
    </div>
  );
}

function OrganizationUnit() {
  const [view, setView] = useState<View>("structure");
  const [selectedNode, setSelectedNode] = useState<OrgUnitTreeNode | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState("");
  const [presetParentId, setPresetParentId] = useState<string | undefined>();
  const [presetParentName, setPresetParentName] = useState<string | undefined>();

  // Add: prefill the parent with the currently selected tree node (if any).
  const addHandler = useCallback(() => {
    setEditId("");
    setPresetParentId(selectedNode?.id);
    setPresetParentName(selectedNode?.name);
    setShowForm(true);
  }, [selectedNode]);

  const editHandler = useCallback((id: string) => {
    setEditId(id);
    setPresetParentId(undefined);
    setPresetParentName(undefined);
    setShowForm(true);
  }, []);

  const closeForm = useCallback(() => setShowForm(false), []);

  return (
    <EntityModuleShell
      title="Organization Structure"
      headerDescription="Business units, directorates, departments and teams"
      headerIcon={<Network className="h-6 w-6 text-primary" />}
      showForm={false}
      hideBack
      onList={closeForm}
      onAdd={addHandler}
    >
      <div className="flex h-full min-h-0 flex-col">
        <ViewToggle view={view} setView={setView} />
        {view === "chart" ? (
          <div className="min-h-0 flex-1 overflow-auto">
            <OrgChart />
          </div>
        ) : (
          <div className="grid min-h-0 flex-1 grid-cols-1 gap-3 md:grid-cols-[auto_minmax(0,1fr)]">
            <OrgTree selectedId={selectedNode?.id} onSelect={setSelectedNode} />
            <OrganizationUnitGrid
              parentId={selectedNode?.id}
              parentName={selectedNode?.name}
              editHandler={editHandler}
            />
          </div>
        )}
      </div>
      {showForm && (
        <OrganizationUnitForm
          id={editId}
          presetParentId={presetParentId}
          presetParentName={presetParentName}
          onClose={closeForm}
          onSaved={closeForm}
        />
      )}
    </EntityModuleShell>
  );
}

export default OrganizationUnit;
