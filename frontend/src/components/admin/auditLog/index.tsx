"use client";
import { useMemo } from "react";
import { ScrollText } from "lucide-react";
import { EntityListShell, EntityModuleShell, useEntityList } from "@/template";
import getAllAuditLog from "@/services/admin/auditLog/getAll";
import type DataTableColumnModel from "@/models/DataTableColumnModel";

const ACTION_TONE: Record<string, string> = {
  Created: "bg-green-100 text-green-700",
  Modified: "bg-blue-100 text-blue-700",
  Reassigned: "bg-amber-100 text-amber-700",
  Deleted: "bg-red-100 text-red-700",
  Rejected: "bg-gray-200 text-gray-700",
};

function AuditLog() {
  const list = useEntityList({ queryKey: "auditLogs", fetchPage: getAllAuditLog });

  const columns = useMemo(
    () =>
      [
        {
          name: "timestamp",
          label: "When",
          sort: true,
          render: (text: string) => (text ? new Date(text).toLocaleString() : ""),
        },
        { name: "entityType", label: "Entity", sort: true },
        { name: "entityName", label: "Name" },
        {
          name: "action",
          label: "Action",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${ACTION_TONE[text] ?? "bg-gray-100 text-gray-700"}`}>
              {text}
            </span>
          ),
        },
        { name: "performedBy", label: "By" },
        {
          name: "changes",
          label: "Changes",
          render: (text: string) =>
            text ? (
              <span className="block max-w-[28rem] truncate font-mono text-xs text-muted" title={text}>
                {text}
              </span>
            ) : (
              ""
            ),
        },
      ] as DataTableColumnModel[],
    [],
  );

  return (
    <EntityModuleShell
      title="Audit Trail"
      headerDescription="History of all structural changes (create, modify, reassign, delete)"
      headerIcon={<ScrollText className="h-6 w-6 text-primary" />}
      showForm={false}
      hideAdd
      hideBack
      onList={() => {}}
      onAdd={() => {}}
    >
      <EntityListShell listKey="auditLogs" listLabel="Audit Trail" columns={columns} {...list} />
    </EntityModuleShell>
  );
}

export default AuditLog;
