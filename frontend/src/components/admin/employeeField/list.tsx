"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllEmployeeField from "@/services/admin/employeeField/getAll";
import deleteEmployeeField from "@/services/admin/employeeField/delete";
import { ownerTypeLabel, fieldOwnerTypeOptions } from "@/constants/orgStructure";
import type { EmployeeFieldModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * Two filters, because `GetAllEmployeeFields` genuinely supports both.
 *
 * ⚠️ They read DIFFERENTLY on the API and the values are not interchangeable: `status` is parsed
 * with `bool.TryParse` (so "true"/"false"), while `ownerType` goes through
 * `Enum.TryParse<EmployeeFieldOwnerType>` (so the enum NAME).
 *
 * ⚠️ The owner options are derived from `fieldOwnerTypeOptions`, the same constant the form's
 * dropdown and `ownerTypeLabel` use — note its ids and names deliberately differ for one entry,
 * "Dependent" showing as "Family", so hardcoding the list here would have sent the label as a value
 * and silently matched nothing.
 */
const FILTERS: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "ownerType",
    label: "Applies to",
    options: [
      { value: "", label: "All forms" },
      ...fieldOwnerTypeOptions.map((o) => ({ value: o.id, label: o.name })),
    ],
  },
  {
    type: "select",
    paramKey: "status",
    label: "Status",
    options: [
      { value: "", label: "All" },
      { value: "true", label: "Active" },
      { value: "false", label: "Inactive" },
    ],
  },
];

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

function EmployeeFieldList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "employeeFields",
    fetchPage: getAllEmployeeField,
    deleteById: deleteEmployeeField,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "label",
          label: "Label",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (text: string, record: EmployeeFieldModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left font-semibold text-primary hover:underline"
            >
              {text || dash}
            </button>
          ),
        },
        {
          name: "ownerType",
          label: "Applies To",
          gridHighlight: true,
          render: (v: string) => {
            const label = ownerTypeLabel(v);
            return label ? <Badge variant="secondary">{label}</Badge> : dash;
          },
        },
        {
          // ⚠️ Monospaced: this is the programmatic KEY the value is stored and merged under
          // (document templates resolve `{{name}}` against it), not prose. Reading it character by
          // character is the whole point, and a proportional font makes l/1/I and O/0 ambiguous.
          name: "name",
          label: "Field Key",
          width: "w-44",
          render: (text: string) =>
            text ? <span className="font-mono text-xs text-foreground">{text}</span> : dash,
        },
        {
          name: "dataType",
          label: "Data Type",
          responsive: "md",
          render: (text: string) => text || dash,
        },
        {
          // Only flagged when it IS required — the common case is not, and a column of "No" is
          // noise that hides the handful that matter.
          name: "isRequired",
          label: "Required",
          responsive: "lg",
          render: (v: unknown) =>
            v === true || v === "true" ? <Badge variant="warning">Required</Badge> : dash,
        },
        {
          name: "isActive",
          label: "Status",
          gridHighlight: true,
          render: (v: unknown) =>
            v === false || v === "false" ? (
              <Badge variant="muted">Inactive</Badge>
            ) : (
              <Badge variant="success">Active</Badge>
            ),
        },
        {
          name: "sortOrder",
          label: "Order",
          responsive: "lg",
          render: (_t: unknown, record: EmployeeFieldModel) =>
            record.sortOrder == null ? dash : (
              <span className="tabular-nums">{record.sortOrder}</span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          render: (_t: unknown, record: EmployeeFieldModel) => (
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
    [editHandler, list.deleteRecord],
  );

  return (
    <EntityListShell
      listKey="employeeFields"
      listLabel="Custom Fields"
      columns={columns}
      listFilters={FILTERS}
      {...list}
    />
  );
}

export default EmployeeFieldList;
