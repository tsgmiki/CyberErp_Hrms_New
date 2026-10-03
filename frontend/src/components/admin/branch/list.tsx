"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllBranch from "@/services/admin/branch/getAll";
import deleteBranch from "@/services/admin/branch/delete";
import type { BranchModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` carrying the STRING "true"/"false" is what the API reads — `GetAllBranches` does
 * `bool.TryParse(request.Status, …)` and filters `IsActive` on it.
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

function BranchList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "branches",
    fetchPage: getAllBranch,
    deleteById: deleteBranch,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Branch",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: BranchModel) => (
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
          width: "w-28",
          render: (text: string) => (
            <span className="font-mono text-xs text-foreground">{text || "—"}</span>
          ),
        },
        {
          // ⚠️ No parent is what makes a branch a ROOT of the branch tree, so the dash carries
          // meaning rather than merely tidying an empty cell.
          name: "parentName",
          label: "Parent Branch",
          render: (text: string) => text || dash,
        },
        {
          // ⚠️ A badge rather than "Yes"/"No", and only on the branches that HAVE the flag.
          // Head office is not a label: `ICurrentUserService.IsHeadOffice()` is read as an
          // authorisation short-circuit in several scope checks, so which branches carry it is
          // worth being able to see at a glance down the column.
          name: "isHeadOffice",
          label: "Head Office",
          gridHighlight: true,
          render: (v: unknown) =>
            v === true || v === "true" ? <Badge variant="info">Head office</Badge> : dash,
        },
        {
          name: "address",
          label: "Address",
          responsive: "lg",
          render: (text: string) =>
            text ? <span className="line-clamp-2 text-muted">{text}</span> : dash,
        },
        {
          name: "isActive",
          label: "Status",
          gridHighlight: true,
          render: (_t: unknown, record: BranchModel) =>
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
          render: (_t: unknown, record: BranchModel) => (
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
      listKey="branches"
      listLabel="Branches"
      columns={columns}
      listFilters={STATUS_FILTER}
      {...list}
    />
  );
}

export default BranchList;
