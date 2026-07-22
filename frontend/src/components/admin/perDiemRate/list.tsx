"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllPerDiemRates from "@/services/admin/perDiemRate/getAll";
import deletePerDiemRate from "@/services/admin/perDiemRate/delete";
import type { PerDiemRateModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function PerDiemRateList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "perDiemRates",
    fetchPage: getAllPerDiemRates,
    deleteById: deletePerDiemRate,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "jobGradeName",
          label: "Job Grade",
          sort: true,
          render: (text: string, record: PerDiemRateModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "tripType", label: "Trip Type" },
        {
          name: "dailyRate",
          label: "Daily Rate",
          render: (_t: unknown, r: PerDiemRateModel) => <span className="tabular-nums">{(r.dailyRate ?? 0).toLocaleString()}</span>,
        },
        { name: "currency", label: "Currency" },
        {
          name: "isActive",
          label: "Active",
          render: (_t: unknown, r: PerDiemRateModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isActive ? "Active" : "Inactive"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: PerDiemRateModel) => (
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

  return <EntityListShell listKey="perDiemRates" listLabel="Per-diem Rates" columns={columns} {...list} />;
}

export default PerDiemRateList;
