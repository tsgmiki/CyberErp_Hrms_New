"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import deleteWorkLocation from "@/services/admin/workLocation/delete";
import type { WorkLocationModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` carrying the STRING "true"/"false" is what the API reads — `GetAllWorkLocations`
 * does `bool.TryParse(request.Status, …)` and filters `IsActive` on it.
 *
 * ⚠️ There is deliberately NO Location Type filter: the API has no `LocationType` parameter, so one
 * would have to be added to the SHARED `GetAllRequest` — disproportionate for a dropdown that would
 * today offer a single value.
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

function WorkLocationList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "workLocations",
    fetchPage: getAllWorkLocation,
    deleteById: deleteWorkLocation,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Location",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: WorkLocationModel) => (
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
          name: "locationType",
          label: "Type",
          sort: true,
          gridHighlight: true,
          render: (text: string) => (text ? <Badge variant="secondary">{text}</Badge> : dash),
        },
        {
          // ⚠️ An em dash here is meaningful rather than cosmetic: no parent is what makes a
          // location a ROOT of the Country → Region → City → Office hierarchy.
          name: "parentName",
          label: "Parent",
          render: (text: string) => text || dash,
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
          render: (_t: unknown, record: WorkLocationModel) =>
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
          render: (_t: unknown, record: WorkLocationModel) => (
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
      listKey="workLocations"
      listLabel="Work Locations"
      columns={columns}
      listFilters={STATUS_FILTER}
      {...list}
    />
  );
}

export default WorkLocationList;
