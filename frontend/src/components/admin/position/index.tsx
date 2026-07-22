import { lazy, memo, useCallback, useState } from "react";
import { Briefcase, Info } from "lucide-react";
import { EntityModuleShell } from "@/template";
import type { OrgUnitTreeNode } from "@/models";

const OrgTree = memo(lazy(() => import("@/components/admin/organizationUnit/orgTree")));
const PositionGrid = memo(lazy(() => import("./grid")));
const PositionForm = memo(lazy(() => import("./form")));

function Position() {
  const [selectedNode, setSelectedNode] = useState<OrgUnitTreeNode | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState("");
  const [presetOrgId, setPresetOrgId] = useState<string | undefined>();
  const [presetOrgName, setPresetOrgName] = useState<string | undefined>();
  const [hint, setHint] = useState(false);

  const onSelect = useCallback((node: OrgUnitTreeNode | null) => {
    setSelectedNode(node);
    setHint(false);
  }, []);

  // Add: capture the selected organization unit; require a tree selection first.
  const addHandler = useCallback(() => {
    if (!selectedNode) {
      setHint(true);
      return;
    }
    setEditId("");
    setPresetOrgId(selectedNode.id);
    setPresetOrgName(selectedNode.name);
    setShowForm(true);
  }, [selectedNode]);

  const editHandler = useCallback((id: string) => {
    setEditId(id);
    setPresetOrgId(undefined);
    setPresetOrgName(undefined);
    setShowForm(true);
  }, []);

  const closeForm = useCallback(() => setShowForm(false), []);

  return (
    <EntityModuleShell
      title="Positions"
      headerDescription="Job positions grouped by organization unit"
      headerIcon={<Briefcase className="h-6 w-6 text-primary" />}
      showForm={false}
      hideBack
      onList={closeForm}
      onAdd={addHandler}
    >
      <div className="flex h-full min-h-0 flex-col gap-2">
        {hint && (
          <div className="mx-1 flex items-center gap-2 rounded border border-info/20 bg-info/15 px-3 py-1.5 text-xs text-info">
            <Info className="h-3.5 w-3.5" />
            Select an organization unit in the tree, then click Add to create a position under it.
          </div>
        )}
        <div className="grid min-h-0 flex-1 grid-cols-1 gap-3 md:grid-cols-[auto_minmax(0,1fr)]">
          <OrgTree selectedId={selectedNode?.id} onSelect={onSelect} />
          <PositionGrid
            organizationUnitId={selectedNode?.id}
            organizationUnitName={selectedNode?.name}
            editHandler={editHandler}
          />
        </div>
      </div>
      {showForm && (
        <PositionForm
          id={editId}
          presetOrganizationUnitId={presetOrgId}
          presetOrganizationUnitName={presetOrgName}
          onClose={closeForm}
          onSaved={closeForm}
        />
      )}
    </EntityModuleShell>
  );
}

export default Position;
