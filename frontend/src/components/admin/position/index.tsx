import { lazy, memo, useCallback, useState } from "react";
import { Briefcase, Info } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";
import type { OrgUnitTreeNode } from "@/models";

const OrgTree = memo(lazy(() => import("@/components/admin/organizationUnit/orgTree")));
const PositionGrid = memo(lazy(() => import("./grid")));
const PositionForm = memo(lazy(() => import("./form")));

function Position() {
  // URL-backed: /position · /position/new · /position/{guid}. As with organizationUnit the form is
  // a modal over the tree, so `showForm` gates the overlay rather than swapping the page.
  const {
    id: editId,
    showForm,
    backHandler: closeForm,
    addHandler: openAdd,
    editHandler: openEdit,
  } = useEntityRouteModule("/position");

  const [selectedNode, setSelectedNode] = useState<OrgUnitTreeNode | null>(null);
  const [presetOrgId, setPresetOrgId] = useState<string | undefined>();
  const [presetOrgName, setPresetOrgName] = useState<string | undefined>();
  const [hint, setHint] = useState(false);

  const onSelect = useCallback((node: OrgUnitTreeNode | null) => {
    setSelectedNode(node);
    setHint(false);
  }, []);

  // Add: capture the selected organization unit; require a tree selection first. A position must
  // belong to a unit, so a cold /position/new (no tree selection) shows the same hint instead of
  // opening an unparented form.
  const addHandler = useCallback(() => {
    if (!selectedNode) {
      setHint(true);
      return;
    }
    setPresetOrgId(selectedNode.id);
    setPresetOrgName(selectedNode.name);
    openAdd();
  }, [selectedNode, openAdd]);

  const editHandler = useCallback((id: string) => {
    setPresetOrgId(undefined);
    setPresetOrgName(undefined);
    openEdit(id);
  }, [openEdit]);

  // A position's owning unit is a hidden field fed only by the tree preset, so a COLD /position/new
  // (pasted URL, refresh — no tree selection) has nothing to parent the new position to. Show the
  // same hint rather than opening a form that would save an unparented position.
  const needsUnit = showForm && !editId && !presetOrgId;

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
        {(hint || needsUnit) && (
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
      {showForm && !needsUnit && (
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
