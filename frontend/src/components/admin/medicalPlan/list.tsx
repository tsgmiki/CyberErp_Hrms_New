"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllMedicalPlans from "@/services/admin/medicalPlan/getAll";
import deleteMedicalPlan from "@/services/admin/medicalPlan/delete";
import type { MedicalPlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function MedicalPlanList({ editHandler }: Props) {
  const list = useEntityList({
    queryKey: "medicalPlans",
    fetchPage: getAllMedicalPlans,
    deleteById: deleteMedicalPlan,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: MedicalPlanModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        {
          name: "coveragePercent",
          label: "Coverage",
          render: (_t: unknown, r: MedicalPlanModel) => <span className="tabular-nums">{r.coveragePercent}%</span>,
        },
        {
          name: "annualCoverageLimit",
          label: "Annual Limit",
          render: (_t: unknown, r: MedicalPlanModel) => <span className="tabular-nums">{r.annualCoverageLimit == null ? "∞" : r.annualCoverageLimit.toLocaleString()}</span>,
        },
        {
          name: "coversDependents",
          label: "Dependents",
          render: (_t: unknown, r: MedicalPlanModel) => (r.coversDependents ? "Yes" : "No"),
        },
        {
          name: "isActive",
          label: "Active",
          render: (_t: unknown, r: MedicalPlanModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isActive ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isActive ? "Active" : "Inactive"}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: MedicalPlanModel) => (
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

  return <EntityListShell listKey="medicalPlans" listLabel="Medical Plans" columns={columns} {...list} />;
}

export default MedicalPlanList;
