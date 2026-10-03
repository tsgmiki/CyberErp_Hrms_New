"use client";

import { useEffect, useMemo, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllPosition from "@/services/admin/position/getAll";
import deletePosition from "@/services/admin/position/delete";
import type { PositionModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  /** Selected organization unit id from the tree; when empty the grid shows all positions. */
  organizationUnitId?: string;
  organizationUnitName?: string;
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `isVacant` is a NULLABLE bool on the API, and the empty "All" value serialises to
 * `isVacant=` which binds back to null — i.e. no filter. Verified against the live endpoint:
 * none/empty → 1162, true → 809, false → 353.
 *
 * ⚠️ Occupied only works because `GetAllPositions` was changed to key on `HasValue`; it previously
 * tested `IsVacant == true`, so asking for false silently returned everything.
 */
const VACANCY_FILTER: ListFilterDefinition[] = [
  {
    type: "select",
    paramKey: "isVacant",
    label: "Vacancy",
    options: [
      { value: "", label: "All" },
      { value: "true", label: "Vacant" },
      { value: "false", label: "Occupied" },
    ],
  },
];

/** An absent value reads as a dash, never a blank cell that looks like a rendering fault. */
const dash = <span className="text-muted">—</span>;

/** Right-hand data grid showing positions of the selected organization unit. */
function PositionGrid({ organizationUnitId, organizationUnitName, editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  // Position GetAll maps `parentId` → OrganizationUnitId filter on the backend.
  const list = useEntityList({
    queryKey: "positions",
    fetchPage: getAllPosition,
    initialParam: organizationUnitId ? { parentId: organizationUnitId } : {},
  });

  useEffect(() => {
    setError(null);
    list.setParam((p) => ({
      ...p,
      parentId: organizationUnitId || undefined,
      skip: 0,
    }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [organizationUnitId]);

  const { mutate: deleteRecord } = useMutation({
    mutationFn: (id: string) => deletePosition(id),
    onSuccess: (result: any) => {
      if (result?.status === "error") {
        setError(result.message || "Delete failed.");
        return;
      }
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["positions"] });
    },
  });

  const columns = useMemo(
    () =>
      [
        {
          // The post is what a reader is looking for; the code identifies it. Lead with the post
          // and carry the code underneath, rather than making the identifier the headline.
          name: "positionClassTitle",
          label: "Position",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: PositionModel) => (
            <button
              type="button"
              onClick={() => record.id && editHandler(record.id)}
              className="text-left"
            >
              <span className="block font-semibold text-primary hover:underline">
                {record.positionClassTitle || t("Untitled position")}
              </span>
              {record.code ? (
                <span className="block font-mono text-xs text-muted">{record.code}</span>
              ) : null}
            </button>
          ),
        },
        {
          // ⚠️ Still carried, even though the tree usually scopes the grid to one unit: with no
          // node selected the header reads "All Positions" and the unit is the only thing telling
          // two identical post titles apart.
          name: "organizationUnitName",
          label: "Organization Unit",
          responsive: "md",
          render: (text: string) => text || dash,
        },
        {
          name: "isVacant",
          label: "Status",
          gridHighlight: true,
          render: (v: unknown) => {
            const vacant = v === true || v === "true";
            return (
              <Badge variant={vacant ? "success" : "muted"}>
                {vacant ? t("Vacant") : t("Occupied")}
              </Badge>
            );
          },
        },
        {
          name: "Action",
          label: "Action",
          gridOmit: true,
          render: (_t: unknown, record: PositionModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              editHandler={editHandler}
              deleteHandler={() => record.id && deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, deleteRecord, t],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      {/* ⚠️ No record count here: the list toolbar below already prints "N records", and a second
          count a few pixels above it is duplication rather than polish. */}
      <div className="min-w-0 border-b border-border px-3 py-2">
        <h3 className="truncate text-sm font-semibold text-foreground">
          {organizationUnitName ? organizationUnitName : t("All Positions")}
        </h3>
        <p className="text-xs text-muted">
          {organizationUnitName
            ? t("Positions belonging directly to this unit")
            : t("Every position, across all units")}
        </p>
      </div>
      {error && (
        <div className="mx-3 mt-2 flex items-center justify-between rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">
            ×
          </button>
        </div>
      )}
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="positions"
          listLabel="Positions"
          columns={columns}
          listFilters={VACANCY_FILTER}
          {...list}
        />
      </div>
    </div>
  );
}

export default PositionGrid;
