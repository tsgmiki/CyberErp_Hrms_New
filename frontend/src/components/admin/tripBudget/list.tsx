"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllTripBudgets from "@/services/admin/tripBudget/getAll";
import deleteTripBudget from "@/services/admin/tripBudget/delete";
import type { TripBudgetModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

interface Props {
  editHandler: (id: string) => void;
  utilizationHandler: (id: string) => void;
}

function TripBudgetList({ editHandler, utilizationHandler }: Props) {
  const list = useEntityList({
    queryKey: "tripBudgets",
    fetchPage: getAllTripBudgets,
    deleteById: deleteTripBudget,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "fiscalYear",
          label: "Fiscal Year",
          sort: true,
          render: (text: string, record: TripBudgetModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        {
          name: "organizationUnitName",
          label: "Unit",
          render: (text: string) => text ?? "Organization-wide",
        },
        {
          name: "amount",
          label: "Amount",
          render: (_t: unknown, r: TripBudgetModel) => <span className="tabular-nums">{money(r.amount)}</span>,
        },
        { name: "notes", label: "Notes" },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: TripBudgetModel) => (
            <GridAction
              id={record.id || ""}
              record={record}
              showAdd={false}
              showEdit
              showDelete
              showAction
              actionName="Utilization"
              actionHandler={(id) => utilizationHandler(id)}
              editHandler={editHandler}
              deleteHandler={() => record.id && list.deleteRecord(record.id)}
            />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, utilizationHandler, list.deleteRecord],
  );

  return <EntityListShell listKey="tripBudgets" listLabel="Travel Budgets" columns={columns} {...list} />;
}

export default TripBudgetList;
