"use client";

import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllFiscalYear from "@/services/admin/fiscalYear/getAll";
import deleteFiscalYear from "@/services/admin/fiscalYear/delete";
import type { FiscalYearModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

const fmt = (v?: string) => (v ? String(v).slice(0, 10) : "");

/**
 * Fiscal years — plain CRUD.
 *
 * The year-end leave **Rollover** used to live on this grid. It moved to Annual Leave Settings,
 * because the carry-forward cap it applies and the expiry rule are both fields of the leave policy,
 * not of the year (logic §12.97). A closed year here is one that has already been rolled over there.
 */
function FiscalYearList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "fiscalYears",
    fetchPage: getAllFiscalYear,
    deleteById: deleteFiscalYear,
  });

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
              <span
                className="rounded-full bg-slate-500/15 px-2 py-0.5 text-xs text-slate-500"
                title="Rolled over from Annual Leave Settings — accepts no further leave activity"
              >
                Closed
              </span>
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
            <GridAction
              id={r.id || ""}
              record={r}
              showAdd={false}
              showEdit={!r.isClosed}
              showDelete={!r.isClosed}
              editHandler={editHandler}
              deleteHandler={() => r.id && list.deleteRecord(r.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="fiscalYears" listLabel="Fiscal Years" columns={columns} {...list} />;
}

export default FiscalYearList;
