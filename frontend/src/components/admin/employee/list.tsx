"use client";

import { useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { FileText } from "lucide-react";
import GridAction from "../../common/gridAction/gridAction";
import Badge, { type BadgeVariant } from "../../common/badge/badge";
import getAllEmployee from "@/services/admin/employee/getAll";
import deleteEmployee from "@/services/admin/employee/delete";
import { employeePhotoUrl } from "@/services/admin/employee/photo";
import { formatBackendDate } from "@/components/util/dateFormater";
import type { EmployeeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";
import GenerateDocumentModal from "./generateDocumentModal";

interface Props {
  /** Selected org unit from the tree; when empty the grid shows all employees. */
  orgUnitId?: string;
  orgUnitName?: string;
  editHandler: (id: string) => void;
}

/**
 * ⚠️ Unlike every other list in this pass, `status` here is an ENUM NAME, not "true"/"false" —
 * `GetAllEmployees` does `Enum.TryParse<EmploymentStatus>(request.Status, …)`. Sending a boolean
 * fails to parse and silently returns the unfiltered set.
 *
 * ⚠️ AND THE EMPTY OPTION IS NOT "ALL". `GetAllEmployees` excludes terminated staff from the
 * directory unless the caller explicitly asks for that status — they belong to the Termination
 * List. Measured against the live API: no filter → 346, Active → 346, Terminated → 145, with 491
 * employees on file. Labelling the blank option "All" would therefore have been untrue, and
 * quietly so: the 145 missing people look like they were never hired.
 */
const STATUS_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "status",
    label: "Employment status",
    options: [
      { value: "", label: "Current staff (default)" },
      { value: "Active", label: "Active" },
      { value: "Probation", label: "Probation" },
      { value: "OnLeave", label: "On leave" },
      { value: "Suspended", label: "Suspended" },
      { value: "Retired", label: "Retired" },
      { value: "Terminated", label: "Terminated only" },
    ],
  },
];

/** Employment status → the shared Badge's variants, which the hand-rolled tones mapped onto anyway. */
const STATUS_VARIANT: Record<string, BadgeVariant> = {
  Active: "success",
  Probation: "info",
  OnLeave: "warning",
  Suspended: "warning",
  Terminated: "error",
  Retired: "muted",
};

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

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
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: EmployeeModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="flex items-center gap-2.5 text-left"
            >
              <Avatar record={record} />
              <span className="min-w-0">
                <span className="block truncate font-semibold text-primary hover:underline">{text}</span>
                <span className="block font-mono text-xs text-muted">{record.employeeNumber}</span>
              </span>
            </button>
          ),
        },
        {
          // ⚠️ Width-capped and clamped. Unit names here run to "Washing, Cleaning, Midia Solution
          // Preparation Section", which left unbounded pushed Status, Documents and Action off the
          // right edge of the panel — the tree already takes a third of the width.
          name: "organizationUnitName",
          label: "Organization Unit",
          responsive: "md",
          width: "w-44",
          render: (text: string) =>
            text ? <span className="line-clamp-2">{text}</span> : dash,
        },
        {
          name: "positionClassTitle",
          label: "Position",
          width: "w-40",
          render: (text: string) =>
            text ? <span className="line-clamp-2">{text}</span> : dash,
        },
        {
          name: "jobGradeName",
          label: "Job Grade",
          responsive: "lg",
          render: (text: string) => (text ? <Badge variant="secondary">{text}</Badge> : dash),
        },
        {
          // ⚠️ Was rendering the RAW backend value — an ISO timestamp in a grid cell.
          name: "hireDate",
          label: "Hire Date",
          sort: true,
          responsive: "md",
          render: (text: string) =>
            text ? <span className="tabular-nums">{formatBackendDate(text)}</span> : dash,
        },
        {
          name: "employmentStatus",
          label: "Status",
          gridHighlight: true,
          render: (text: string) =>
            text ? <Badge variant={STATUS_VARIANT[text] ?? "muted"}>{text}</Badge> : dash,
        },
        {
          // ⚠️ Documents used to own a whole column of its own. It is a per-ROW action, so it sits
          // with Edit and Delete — which also buys back the width that was pushing Status off the
          // right edge of the panel next to the tree.
          name: "Action",
          label: "Action",
          gridOmit: true,
          render: (_t: unknown, record: EmployeeModel) => (
            <span className="flex items-center justify-end gap-1">
              <button
                type="button"
                onClick={() => setDocFor(record)}
                title={t("Generate Document")}
                // ⚠️ `hover:opacity-80`, not the `hover:border-primary hover:text-primary` this
                // had. BOTH of those are unregistered palette utilities that emit nothing, so the
                // button had no hover feedback at all. Opacity is core Tailwind and needs no theme
                // entry.
                className="inline-flex items-center rounded p-1 text-foreground transition-opacity hover:opacity-80"
              >
                <FileText size={15} />
              </button>
              <GridAction
                id={record.id || ""}
                record={record}
                showAdd={false}
                showEdit
                showDelete
                editHandler={editHandler}
                deleteHandler={() => record.id && list.deleteRecord(record.id)}
              />
            </span>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      {/* ⚠️ No record count here: the list toolbar below already prints "N records". */}
      <div className="min-w-0 border-b border-border px-3 py-2">
        <h3 className="truncate text-sm font-semibold text-foreground">
          {orgUnitName ? orgUnitName : t("All Employees")}
        </h3>
        <p className="text-xs text-muted">
          {orgUnitName
            ? t("Employees posted to this unit")
            : t("Everyone on the register, across all units")}
        </p>
      </div>
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="employees"
          listLabel="Employees"
          columns={columns}
          listFilters={STATUS_FILTER}
          {...list}
        />
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
