"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllJobCategory from "@/services/admin/jobCategory/getAll";
import deleteJobCategory from "@/services/admin/jobCategory/delete";
import type { JobCategoryModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` carrying the STRING "true"/"false" is what the API reads — `GetAllJobCategories`
 * does `bool.TryParse(request.Status, …)` and filters `IsActive` on it, exactly as the Position
 * Class list does. Any other key or value would render a filter that changes nothing.
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

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

function JobCategoryList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "jobCategories",
    fetchPage: getAllJobCategory,
    deleteById: deleteJobCategory,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Category",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: JobCategoryModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left font-semibold text-primary hover:underline"
            >
              {record.name || dash}
            </button>
          ),
        },
        {
          name: "code",
          label: "Code",
          sort: true,
          width: "w-32",
          render: (text: string) => (
            <span className="font-mono text-xs text-foreground">{text || "—"}</span>
          ),
        },
        {
          name: "description",
          label: "Description",
          responsive: "lg",
          // ⚠️ Clamped rather than left to wrap. A category description is free text, and one long
          // paragraph would otherwise set the row height for the whole table.
          render: (text: string) =>
            text ? <span className="line-clamp-2 text-muted">{text}</span> : dash,
        },
        {
          name: "isActive",
          label: "Status",
          gridHighlight: true,
          render: (_t: unknown, record: JobCategoryModel) =>
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
          render: (_t: unknown, record: JobCategoryModel) => (
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
      listKey="jobCategories"
      listLabel="Job Categories"
      columns={columns}
      listFilters={STATUS_FILTER}
      {...list}
    />
  );
}

export default JobCategoryList;
