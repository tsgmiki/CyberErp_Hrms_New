"use client";

import { useEffect, useMemo, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import Badge from "../../common/badge/badge";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import deleteOrganizationUnit from "@/services/admin/organizationUnit/delete";
import type { OrganizationUnitModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import type { ListFilterDefinition } from "@/components/common/searchBar/listFilterTypes";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  /** Currently selected tree node; when empty the grid shows root units. */
  parentId?: string;
  parentName?: string;
  editHandler: (id: string) => void;
}

/**
 * ⚠️ `status` carrying the STRING "true"/"false" is what the API reads — `GetAllOrganizationUnits`
 * does `bool.TryParse(request.Status, …)` and filters `IsActive` on it.
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

/** Right-hand data grid showing the children of the selected tree node. */
function OrganizationUnitGrid({ parentId, parentName, editHandler }: Props) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);

  const list = useEntityList({
    queryKey: "organizationUnits",
    fetchPage: getAllOrganizationUnit,
    initialParam: parentId ? { parentId } : { isRoot: true },
  });

  // Re-scope the grid whenever the selected tree node changes.
  useEffect(() => {
    setError(null);
    list.setParam((p) => ({
      ...p,
      parentId: parentId || undefined,
      isRoot: !parentId,
      skip: 0,
    }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [parentId]);

  const { mutate: deleteRecord } = useMutation({
    mutationFn: (id: string) => deleteOrganizationUnit(id),
    onSuccess: (result: any) => {
      if (result?.status === "error") {
        setError(result.message || "Delete failed.");
        return;
      }
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["organizationUnits"] });
      queryClient.invalidateQueries({ queryKey: ["organizationTree"] });
    },
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Unit",
          sort: true,
          gridPrimary: true,
          gridHideLabel: true,
          render: (_t: unknown, record: OrganizationUnitModel) => (
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
          // Four real values here — BusinessUnit, Directorate, Department, Team — so the badge
          // carries the level of the unit rather than decorating a single repeated word.
          name: "unitType",
          label: "Type",
          sort: true,
          gridHighlight: true,
          render: (text: string) => (text ? <Badge variant="secondary">{text}</Badge> : dash),
        },
        {
          // ⚠️ Recorded for 2 of 121 units, so this column is mostly dashes — kept because an
          // establishment figure is load-bearing where it IS set (workforce planning reads it),
          // and a dash states "not established" far more clearly than a blank cell.
          name: "allocatedHeadcount",
          label: "Headcount",
          responsive: "md",
          render: (_t: unknown, record: OrganizationUnitModel) =>
            record.allocatedHeadcount == null ? dash : (
              <span className="tabular-nums">{record.allocatedHeadcount}</span>
            ),
        },
        {
          name: "isActive",
          label: "Status",
          gridHighlight: true,
          render: (_t: unknown, record: OrganizationUnitModel) =>
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
          render: (_t: unknown, record: OrganizationUnitModel) => (
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
    [editHandler, deleteRecord],
  );

  return (
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-card">
      {/* ⚠️ No record count here: the list toolbar below already prints "N records". */}
      <div className="min-w-0 border-b border-border px-3 py-2">
        <h3 className="truncate text-sm font-semibold text-foreground">
          {parentName ? parentName : t("Root Units")}
        </h3>
        <p className="text-xs text-muted">
          {parentName
            ? t("Units reporting directly to this one")
            : t("Top-level units, with no parent above them")}
        </p>
      </div>
      {/*
        ⚠️ Palette classes, not raw `red-300`/`red-50`/`red-700`. The raw Tailwind reds render, but
        they are fixed values that ignore the theme and will not follow dark mode.
        ⚠️ And `border-error/20`, NOT the `border-error/30` the Positions grid uses for the same
        banner — only /20 is written into theme.css, so /30 emits nothing and that border is
        invisible wherever it appears (5+ other call sites).
      */}
      {error && (
        <div className="mx-3 mt-2 flex items-center justify-between rounded border border-error/20 bg-error/15 px-3 py-2 text-xs text-error">
          <span>{error}</span>
          <button type="button" onClick={() => setError(null)} className="font-semibold">
            ×
          </button>
        </div>
      )}
      <div className="min-h-0 flex-1 overflow-auto">
        <EntityListShell
          listKey="organizationUnits"
          listLabel="Organization Units"
          columns={columns}
          listFilters={STATUS_FILTER}
          {...list}
        />
      </div>
    </div>
  );
}

export default OrganizationUnitGrid;
