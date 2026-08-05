"use client";
import { useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import GridAction from "../../common/gridAction/gridAction";
import { getAllGuarantees, getMyGuarantees, deleteGuarantee, getGuaranteeDashboard } from "@/services/admin/employeeGuarantee";
import type { EmployeeGuaranteeModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { guaranteeTypeLabel, guaranteeStatusLabel } from "@/constants/orgStructure";
import { EntityListShell, useEntityList } from "@/template";

const STATUS_TONE: Record<string, string> = {
  Active: "bg-success/15 text-success",
  Released: "bg-muted/30 text-muted",
  PendingApproval: "bg-warning/15 text-warning",
  Rejected: "bg-error/15 text-error",
};

const money = (v?: number) =>
  (v ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** HC307 — headline chips above the register (own slice for non-admin callers). */
function DashboardStrip() {
  const { t } = useTranslation();
  const { data } = useQuery({ queryKey: ["guaranteeDashboard"], queryFn: getGuaranteeDashboard, staleTime: 60_000 });
  if (!data) return null;
  const chip = (label: string, value: string | number, tone = "border-border bg-secondary/30 text-foreground") => (
    <span className={`rounded-full border px-2.5 py-1 text-xs ${tone}`}>
      {label}: <b className="tabular-nums">{value}</b>
    </span>
  );
  return (
    <div className="mb-3 flex flex-wrap items-center gap-2">
      {chip(t("Total"), data.total)}
      {chip(t("Active"), data.active, "border-success/30 bg-success/10 text-foreground")}
      {chip(t("Pending"), data.pendingApproval, "border-warning/30 bg-warning/10 text-foreground")}
      {chip(t("Released"), data.released)}
      {chip(t("Active exposure"), money(data.activeAmount), "border-primary/30 bg-primary/10 text-foreground")}
      {data.expiringSoon > 0 &&
        chip(t("Expiring in 60 days"), data.expiringSoon, "border-error/30 bg-error/10 text-foreground")}
    </div>
  );
}

function GuaranteeList({
  editHandler,
  mine = false,
  employeeId,
}: {
  editHandler: (id: string) => void;
  mine?: boolean;
  /** Scope to ONE employee (the profile Guarantees tab) — hides the employee column + chips. */
  employeeId?: string;
}) {
  const list = useEntityList({
    queryKey: mine ? "myGuarantees" : "employeeGuarantees",
    fetchPage: mine ? getMyGuarantees : getAllGuarantees,
    deleteById: deleteGuarantee,
    ...(employeeId ? { initialParam: { employeeId } } : {}),
  });

  const columns = useMemo(
    () =>
      [
        ...(!mine && !employeeId
          ? [{
              name: "employeeName", label: "Employee", sort: true,
              render: (v: string, r: EmployeeGuaranteeModel) => (
                <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
                  <span className="block font-semibold">{v ?? "—"}</span>
                  <span className="block text-xs text-muted">{r.employeeNumber}</span>
                </button>
              ),
            }]
          : []),
        {
          name: "externalOrganization", label: "Organization", sort: true,
          render: (v: string, r: EmployeeGuaranteeModel) => (
            <button type="button" onClick={() => r.id && editHandler(r.id)} className="text-left">
              <span className="block font-semibold">{v}</span>
              <span className="block text-xs text-muted">{guaranteeTypeLabel(r.type)}</span>
            </button>
          ),
        },
        { name: "beneficiaryName", label: "Beneficiary" },
        {
          name: "amount", label: "Amount",
          render: (v: number) => <span className="block text-right tabular-nums">{money(v)}</span>,
        },
        {
          name: "startDate", label: "Period",
          render: (_v: string, r: EmployeeGuaranteeModel) =>
            `${(r.startDate ?? "").slice(0, 10)} → ${r.endDate ? r.endDate.slice(0, 10) : "open"}`,
        },
        {
          name: "status", label: "Status",
          render: (v: string) => (
            <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[v] ?? "bg-muted/30 text-muted"}`}>
              {guaranteeStatusLabel(v)}
            </span>
          ),
        },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, r: EmployeeGuaranteeModel) => (
            <GridAction id={r.id || ""} record={r} showAdd={false} showEdit
              showDelete={r.status !== "Released"}
              editHandler={editHandler} deleteHandler={() => r.id && list.deleteRecord(r.id)} />
          ),
        },
      ] as DataTableColumnModel[],
    [editHandler, list.deleteRecord, mine, employeeId],
  );

  return (
    <div>
      {!mine && !employeeId && <DashboardStrip />}
      <EntityListShell
        listKey={mine ? "myGuarantees" : "employeeGuarantees"}
        listLabel={mine ? "My Guarantee Commitments" : "Guarantee Commitments"}
        columns={columns}
        {...list}
      />
    </div>
  );
}

export default GuaranteeList;
