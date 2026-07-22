"use client";
import { useMemo } from "react";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import getAllBenefitPlans from "@/services/admin/benefitPlan/getAll";
import deleteBenefitPlan from "@/services/admin/benefitPlan/delete";
import type { BenefitPlanModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { EntityListShell, useEntityList } from "@/template";

interface Props {
  editHandler: (id: string) => void;
}

function BenefitPlanList({ editHandler }: Props) {
  const { t } = useTranslation();
  const list = useEntityList({
    queryKey: "benefitPlans",
    fetchPage: getAllBenefitPlans,
    deleteById: deleteBenefitPlan,
  });

  const columns = useMemo(
    () =>
      [
        {
          name: "name",
          label: "Name",
          sort: true,
          render: (text: string, record: BenefitPlanModel) => (
            <button type="button" onClick={() => record.id && editHandler(record.id)} className="font-semibold">
              {text}
            </button>
          ),
        },
        { name: "category", label: "Category", render: (v: string) => t(v ?? "") },
        {
          name: "employeeContributionRate",
          label: "Employee",
          render: (_t: unknown, r: BenefitPlanModel) => (
            <span className="tabular-nums">{r.employeeContributionRate}{r.employeeContributionMethod === "PercentOfBase" ? "%" : ""}</span>
          ),
        },
        {
          name: "employerContributionRate",
          label: "Employer",
          render: (_t: unknown, r: BenefitPlanModel) => (
            <span className="tabular-nums">{r.employerContributionRate}{r.employerContributionMethod === "PercentOfBase" ? "%" : ""}</span>
          ),
        },
        {
          name: "isEnrollmentOpen",
          label: "Enrollment",
          render: (_t: unknown, r: BenefitPlanModel) => (
            <span className={`rounded-full px-2 py-0.5 text-xs font-semibold ${r.isEnrollmentOpen ? "bg-success/15 text-success" : "bg-muted/30 text-muted"}`}>
              {r.isEnrollmentOpen ? t("Open") : t("Closed")}
            </span>
          ),
        },
        {
          name: "Action",
          label: "Action",
          render: (_t: unknown, record: BenefitPlanModel) => (
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
    [editHandler, list.deleteRecord, t],
  );

  return <EntityListShell listKey="benefitPlans" listLabel="Benefit Plans" columns={columns} {...list} />;
}

export default BenefitPlanList;
