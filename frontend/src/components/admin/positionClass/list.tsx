"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import deletePositionClass from "@/services/admin/positionClass/delete";
import type { PositionClassModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` is the parameter the API already reads, and it expects the STRING "true"/"false" —
 * `GetAllPositionClasses` does `bool.TryParse(request.Status, …)` and filters `IsActive` on it.
 * Any other key or value would render a filter that looks like it works and changes nothing.
 */
const STATUS_FILTER: ListFilterDefinition[] = [
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

/** An absent value reads as a dash, never as a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

const money = (v?: number | null) =>
  v == null ? dash : (
    <span className="font-medium tabular-nums">
      {Number(v).toLocaleString(undefined, { minimumFractionDigits: 2 })}
    </span>
  );

function PositionClassList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "positionClasses",
    fetchPage: getAllPositionClass,
    deleteById: deletePositionClass,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "title",
          label: "Position",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          // ⚠️ The Amharic title rides under the English one instead of holding a column of its
          // own: only 1 of 814 classes has a TitleA, so a dedicated column was an empty strip
          // across the whole grid. Here it simply does not appear until there is one.
          render: (_t: unknown, record: PositionClassModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left"
            >
              <span className="block font-semibold text-primary hover:underline">
                {record.title}
              </span>
              {record.titleA ? (
                <span className="block text-xs text-muted">{record.titleA}</span>
              ) : null}
            </button>
          ),
        },
        {
          name: "code",
          label: "Code",
          sort: true,
          width: "w-28",
          render: (text: string) => (
            <span className="font-mono text-xs text-foreground">{text || "—"}</span>
          ),
        },
        {
          name: "jobGradeName",
          label: "Grade",
          gridHighlight: true,
          render: (text: string) => (text ? <Badge variant="secondary">{text}</Badge> : dash),
        },
        {
          name: "salary",
          label: "Salary",
          render: (_t: unknown, record: PositionClassModel) => money(record.salary),
        },
        {
          name: "jobCategoryName",
          label: "Category",
          responsive: "lg",
          render: (text: string) => text || dash,
        },
        {
          name: "allocatedHeadcount",
          label: "Headcount",
          responsive: "md",
          render: (_t: unknown, record: PositionClassModel) =>
            record.allocatedHeadcount == null ? dash : (
              <span className="tabular-nums">{record.allocatedHeadcount}</span>
            ),
        },
        {
          name: "isActive",
          label: "Status",
          gridHighlight: true,
          render: (_t: unknown, record: PositionClassModel) =>
            record.isActive === false ? (
              <Badge variant="muted">Inactive</Badge>
            ) : (
              <Badge variant="success">Active</Badge>
            ),
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          render: (_t: unknown, record: PositionClassModel) => (
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
      listKey="positionClasses"
      listLabel="Position Classes"
      columns={columns}
      listFilters={STATUS_FILTER}
      {...list}
    />
  );
}

export default PositionClassList;
