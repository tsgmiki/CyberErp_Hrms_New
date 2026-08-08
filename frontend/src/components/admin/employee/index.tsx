import { lazy, memo, useState } from "react";
import { Users } from "lucide-react";
import { EntityModuleShell, useEntityRouteModule } from "@/template";
import type { OrgUnitTreeNode } from "@/models";

const OrgTree = memo(lazy(() => import("@/components/admin/organizationUnit/orgTree")));
const EmployeeList = memo(lazy(() => import("./list")));
const EmployeeProfile = memo(lazy(() => import("./profile")));

function Employee() {
  // URL-backed: /employee (tree + list) · /employee/new · /employee/{guid} (profile).
  // The org-tree selection stays local — it filters the list, it isn't the record being edited,
  // so it has no place in the URL.
  const {
    id: editId,
    setId: setEditId,
    showForm: showProfile,
    backHandler,
    addHandler,
    editHandler,
  } = useEntityRouteModule("/employee");

  const [selectedNode, setSelectedNode] = useState<OrgUnitTreeNode | null>(null);

  return (
    <EntityModuleShell
      title="Employees"
      headerDescription="Employee master data, education, experience and family details"
      headerIcon={<Users className="h-6 w-6 text-primary" />}
      showForm={showProfile}
      onList={backHandler}
      onAdd={addHandler}
    >
      {showProfile ? (
        <EmployeeProfile
          id={editId}
          setId={setEditId}
          onBack={backHandler}
          orgUnitId={selectedNode?.id}
          orgUnitName={selectedNode?.name}
        />
      ) : (
        <div className="grid h-full min-h-0 grid-cols-1 gap-3 md:grid-cols-[auto_minmax(0,1fr)]">
          <OrgTree selectedId={selectedNode?.id} onSelect={setSelectedNode} />
          <EmployeeList
            orgUnitId={selectedNode?.id}
            orgUnitName={selectedNode?.name}
            editHandler={editHandler}
          />
        </div>
      )}
    </EntityModuleShell>
  );
}

export default Employee;
