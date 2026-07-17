"use client";

import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { FileText } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import getAllEmployee from "@/services/admin/employee/getAll";
import deleteEmployee from "@/services/admin/employee/delete";
import { employeePhotoUrl } from "@/services/admin/employee/photo";
import type { EmployeeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";
import GenerateDocumentModal from "./generateDocumentModal";

interface Props {
  /** Selected org unit from the tree; when empty the grid shows all employees. */
  orgUnitId?: string;
  orgUnitName?: string;
  editHandler: (id: string) => void;
}

const STATUS_TONE: Record<string, string> = {
  Active: "bg-success/15 text-success",
  Probation: "bg-info/15 text-info",
  OnLeave: "bg-warning/15 text-warning",
  Suspended: "bg-warning/15 text-warning",
  Terminated: "bg-error/15 text-error",
  Retired: "bg-muted/30 text-muted",
};

function initialsOf(name?: string) {
  return (
    name
      ?.split(/\s+/)
      .map((p) => p[0])
      .join("")
      .toUpperCase()
      .slice(0, 2) || "?"
  );
}

function Avatar({ record }: { record: EmployeeModel }) {
  if (record.photoUrl && record.id) {
    return (
      <img
        src={employeePhotoUrl(record.id)}
        alt=""
        className="h-8 w-8 shrink-0 rounded-full border border-border object-cover"
      />
    );
  }
  return (
    <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
      {initialsOf(record.fullName)}
    </span>
  );
}

function EmployeeList({ orgUnitId, orgUnitName, editHandler }: Props) {
  const { t } = useTranslation();
  const [docFor, setDocFor] = useState<EmployeeModel | null>(null);
  const list = useEntityList({
    queryKey: "employees",
    fetchPage: getAllEmployee,
    deleteById: deleteEmployee,
    initialParam: orgUnitId ? { parentId: orgUnitId } : {},
  });

  // Re-scope whenever the selected tree node changes.
  useEffect(() => {
    list.setParam((p) => ({ ...p, parentId: orgUnitId || undefined, skip: 0 }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [orgUnitId]);

  const columns = useMemo(
    () =>
      [
        {
          name: "fullName",
          label: "Employee",
          sort: true,
          render: (text: string, record: EmployeeModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="flex items-center gap-2.5 text-left"
            >
              <Avatar record={record} />
              <span className="min-w-0">
                <span className="block truncate font-semibold">{text}</span>
                <span className="block text-xs text-muted">{record.employeeNumber}</span>
              </span>
            </button>
          ),
        },
        { name: "organizationUnitName", label: "Organization Unit" },
        { name: "positionClassTitle", label: "Position" },
        { name: "jobGradeName", label: "Job Grade" },
        { name: "hireDate", label: "Hire Date" },
        {
          name: "employmentStatus",
          label: "Status",
          render: (text: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[text] ?? "bg-muted/30 text-muted"}`}>
              {text}
            </span>
          ),
        },
        {
          name: "Documents",
          label: "Documents",
          render: (_t: unknown, record: EmployeeModel) => (
            <button
              type="button"
              onClick={() => setDocFor(record)}
              title={t("Generate Document")}
              className="inline-flex items-center gap-1 rounded border border-border px-2 py-1 text-xs text-foreground hover:border-primary hover:text-primary"
            >
              <FileText size={14} />
            </button>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: EmployeeModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      <div className="border-b border-border px-3 py-2 text-sm font-semibold text-foreground">
        {orgUnitName ? `${t("Employees in")}: ${orgUnitName}` : t("All Employees")}
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell listKey="employees" listLabel="Employees" columns={columns} {...list} />
      </div>
      {docFor?.id && (
        <GenerateDocumentModal
          employeeId={docFor.id}
          employeeName={docFor.fullName}
          onClose={() => setDocFor(null)}
        />
      )}
    </div>
  );
}

export default EmployeeList;
