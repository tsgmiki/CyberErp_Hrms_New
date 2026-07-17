"use client";
import { useMemo } from "react";
import GridAction from "../../common/gridAction/gridAction";
import getAllSuccessionPlan from "@/services/admin/successionPlan/getAll";
import deleteSuccessionPlan from "@/services/admin/successionPlan/delete";
import type { SuccessionPlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

function SuccessionPlanList({ editHandler }: { editHandler: (id: string) => void }) {
  const list = useEntityList({ queryKey: "successionPlans", fetchPage: getAllSuccessionPlan, deleteById: deleteSuccessionPlan });
  const columns = useMemo(
    () =>
      [
        { name: "name", label: "Plan", sort: true, render: (t: string, r: SuccessionPlanModel) => (
          <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
            <span className="block font-semibold">{t}</span>
            <span className="block text-xs text-muted">{r.roleTitle}</span>
          </button>) },
        { name: "horizon", label: "Horizon" },
        { name: "status", label: "Status" },
        { name: "Action", label: "Action", render: (_t: unknown, r: SuccessionPlanModel) => (
          <GridAction id={r.id || ""} record={r} showAdd={false} showEdit showDelete
            editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />) },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord],
  );
  return <EntityListShell listKey="successionPlans" listLabel="Succession Plans" columns={columns} {...list} />;
}
export default SuccessionPlanList;
