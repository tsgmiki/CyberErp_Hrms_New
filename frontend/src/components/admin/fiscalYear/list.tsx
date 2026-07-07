"use client";

import { useMemo } from "react";
import { useQueryClient } from "@tanstack/react-query";
import GridAction from "../../common/gridAction/gridAction";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import deleteFiscalYear from "@/services/admin/fiscalYear/delete";
import rolloverFiscalYear from "@/services/admin/fiscalYear/rollover";
import type { FiscalYearModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

function FiscalYearList({ editHandler }: Props) {
  const queryClient = useQueryClient();
  const list = useEntityList({
    queryKey: "fiscalYears",
    fetchPage: getAllFiscalYear,
    deleteById: deleteFiscalYear,
  });

  const doRollover = async (r: FiscalYearModel) => {
    if (!r.id) return;
    if (!window.confirm(`Roll all leave balances of "${r.name}" into the next fiscal year and close it? This cannot be undone.`)) return;
    const result = await rolloverFiscalYear(r.id);
    window.alert(result?.message ?? "Rollover complete");
    queryClient.invalidateQueries({ queryKey: ["fiscalYears"] });
  };

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, r: FiscalYearModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "startDate", label: "Start", render: (_t: unknown, r: FiscalYearModel) => fmt(r.startDate) },
        { name: "endDate", label: "End", render: (_t: unknown, r: FiscalYearModel) => fmt(r.endDate) },
        {
          name: "isActive",
          label: "Status",
          render: (_t: unknown, r: FiscalYearModel) =>
            r.isClosed ? (
              <span className="rounded-full bg-slate-500/15 px-2 py-0.5 text-xs text-slate-500">Closed</span>
            ) : r.isActive ? (
              <span className="rounded-full bg-emerald-500/15 px-2 py-0.5 text-xs text-emerald-600">Active</span>
            ) : (
              <span className="rounded-full bg-amber-500/15 px-2 py-0.5 text-xs text-amber-600">Open</span>
            ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, r: FiscalYearModel) => (
            <div className="flex items-center gap-2">
              {!r.isClosed && (
                <button
                  type="button"
                  onClick={() => doRollover(r)}
                  className="rounded-md border border-border px-2 py-1 text-xs hover:bg-primary/10"
                  title="Carry remaining leave into the next fiscal year and close this one"
                >
                  Rollover
                </button>
              )}
              <GridAction
                id={r.id || ""}
                record={r}
                showAdd={false}
                showEdit={!r.isClosed}
                showDelete={!r.isClosed}
                editHandler={editHandler}
                deleteHandler={() => r.id && list.deleteRecord(r.id)}
              />
            </div>
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="fiscalYears" listLabel="Fiscal Years" columns={columns} {...list} />;
}

export default FiscalYearList;
