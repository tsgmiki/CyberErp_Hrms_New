"use client";
import { memo, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { HeartPulse, Plus, UserPlus, Ban, PauseCircle, PlayCircle, Trash2 } from "lucide-react";
import {
  getEmployeeMedicalEnrollments, saveMedicalEnrollment, setMedicalEnrollmentStatus, deleteMedicalEnrollment,
  addMedicalBeneficiary, removeMedicalBeneficiary, getAllMedicalPlans,
} from "@/services/admin/medical";
import { getDependents } from "@/services/admin/employee/children";
import type { MedicalEnrollmentModel } from "@/models";
import type DataTableColumnModel from "@/models/DataTableColumnModel";
import { parameterInitialData } from "@/constants/initialization";
import { EntityModuleShell } from "@/template";
import EmployeePicker from "@/components/common/employeePicker";
import DropDownField from "@/components/ui/dropDownField";
import DateField from "@/components/ui/dateField";
import ButtonField from "@/components/ui/buttonField";
import DataTableProvider from "@/components/common/dataTableProvider/dataTableProvider";
import GridAction from "@/components/common/gridAction/gridAction";
import DetailSection from "@/components/common/detailSection";

const CAT_OPTIONS = ["Spouse", "Child", "Parent", "Pensioner", "Other"].map((c) => ({ id: c, name: c }));
/** Map a Family relationship to the medical BeneficiaryCategory; anything else (Sibling, …) → Other. */
const REL_TO_CATEGORY: Record<string, string> = { Spouse: "Spouse", Child: "Child", Parent: "Parent" };
const toCategory = (rel?: string) => REL_TO_CATEGORY[rel ?? ""] ?? "Other";
const iso = (d: Date) => d.toISOString().slice(0, 10);
const statusBadge = (s?: string) => (s === "Active" ? "bg-success/15 text-success" : s === "Suspended" ? "bg-warning/15 text-warning" : "bg-error/15 text-error");

/** HC235/HC237 — HR per-employee medical enrollment + dependents/beneficiaries. */
function MedicalEnrollment() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [emp, setEmp] = useState<{ id: string; name: string } | null>(null);
  const [msg, setMsg] = useState("");
  const [newEnroll, setNewEnroll] = useState({ medicalPlanId: "", medicalPlanName: "", coverageStart: iso(new Date()) });
  const [ben, setBen] = useState<Record<string, { employeeDependentId: string; dependentName: string; category: string }>>({});

  const empId = emp?.id;
  const { data: enrollments } = useQuery({ queryKey: ["medEnrollments", empId], queryFn: () => getEmployeeMedicalEnrollments(empId!), enabled: !!empId });
  const { data: plans } = useQuery({ queryKey: ["medPlanOpts"], queryFn: () => getAllMedicalPlans({ ...parameterInitialData, take: 200, status: "true" }), staleTime: 60_000 });
  const planOptions = (plans?.data ?? []).map((x) => ({ id: x.id!, name: x.name! }));
  // Beneficiaries are drawn from the employee's existing Family records (EmployeeDependent) — never typed in here.
  const { data: dependents } = useQuery({ queryKey: ["empDependents", empId], queryFn: () => getDependents(empId!), enabled: !!empId });

  const refresh = (m: string) => { setMsg(m); queryClient.invalidateQueries({ queryKey: ["medEnrollments", empId] }); };

  const enroll = async () => {
    if (!empId || !newEnroll.medicalPlanId) return;
    const res = await saveMedicalEnrollment({ employeeId: empId, medicalPlanId: newEnroll.medicalPlanId, coverageStart: newEnroll.coverageStart });
    refresh(res.message);
    if (res.ok) setNewEnroll({ medicalPlanId: "", medicalPlanName: "", coverageStart: iso(new Date()) });
  };
  const addBen = async (enrollmentId: string) => {
    const b = ben[enrollmentId];
    if (!b || !b.employeeDependentId) return;
    // Only the family-record link + category are sent; the backend snapshots name/DOB/relationship from EmployeeDependent.
    const res = await addMedicalBeneficiary({ medicalEnrollmentId: enrollmentId, category: b.category, employeeDependentId: b.employeeDependentId });
    refresh(res.message);
    if (res.ok) setBen((p) => ({ ...p, [enrollmentId]: { employeeDependentId: "", dependentName: "", category: "Child" } }));
  };
  const setBenField = (id: string, p: Partial<{ employeeDependentId: string; dependentName: string; category: string }>) =>
    setBen((s) => {
      const cur = s[id] ?? { employeeDependentId: "", dependentName: "", category: "Child" };
      return { ...s, [id]: { ...cur, ...p } };
    });

  const beneficiaryColumns = useMemo(
    () =>
      [
        { name: "fullName", label: "Beneficiary", render: (_t: unknown, b: any) => b.fullName || (b.category === "Employee" ? "—" : "—") },
        { name: "category", label: "Category", render: (v: string) => t(v ?? "") },
        { name: "relationship", label: "Relationship", render: (v: string) => v ?? "—" },
        { name: "dateOfBirth", label: "DOB", render: (v: string) => v?.slice(0, 10) ?? "—" },
        {
          name: "Action", label: "Action",
          render: (_t: unknown, b: any) =>
            b.category === "Employee" ? null : (
              <GridAction id={b.id} record={b} showAdd={false} showEdit={false} showDelete deleteHandler={(id) => removeMedicalBeneficiary(id).then((r) => refresh(r.message))} />
            ),
        },
      ] as DataTableColumnModel[],
    [t],
  );

  return (
    <EntityModuleShell
      title="Medical Enrollment"
      headerDescription="Enroll an employee in a medical plan and register covered beneficiaries"
      headerIcon={<HeartPulse className="h-6 w-6 text-primary" />}
      tableTitle="Medical Enrollment"
      hideAdd
      hideBack
      showForm={false}
      onList={() => undefined}
      onAdd={() => undefined}
    >
      <div className="m-2 flex min-h-0 flex-1 flex-col gap-3">
        <div className="max-w-md"><EmployeePicker value={emp?.id} displayValue={emp?.name} onSelect={(id, name) => setEmp({ id, name })} placeholder={t("Select an employee…")} /></div>
        {msg && <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">{msg}</p>}

        {emp && (
          <div className="min-h-0 flex-1 space-y-3 overflow-auto">
            <DetailSection title="New enrollment">
              <div className="flex flex-wrap items-end gap-2">
                <div className="min-w-52 flex-1">
                  <DropDownField type="dropDown" compact name="newPlan" label="Plan" value={newEnroll.medicalPlanId} displayValue={newEnroll.medicalPlanName} data={planOptions} placeholder="Select plan…" onSelect={(_n, item) => setNewEnroll((p) => ({ ...p, medicalPlanId: item.id, medicalPlanName: item.name }))} />
                </div>
                <div>
                  <DateField type="date" compact name="coverageStart" label="Coverage start" value={newEnroll.coverageStart} onChange={(e) => setNewEnroll((p) => ({ ...p, coverageStart: e.target.value }))} />
                </div>
                <ButtonField value="Enroll" variant="primary" icon={<Plus size={14} />} disabled={!newEnroll.medicalPlanId} onClick={enroll} />
              </div>
            </DetailSection>

            {(enrollments ?? []).map((en: MedicalEnrollmentModel) => {
              const rows = (en.beneficiaries ?? []).map((b) => ({ ...b, fullName: b.fullName || (b.category === "Employee" ? en.employeeName : "") }));
              const addedDepIds = new Set((en.beneficiaries ?? []).map((b) => b.employeeDependentId).filter(Boolean));
              const depOptions = (dependents ?? []).filter((d) => d.id && !addedDepIds.has(d.id)).map((d) => ({ id: d.id!, name: `${d.fullName}${d.relationship ? ` (${d.relationship})` : ""}` }));
              return (
                <div key={en.id} className="rounded-lg border border-border bg-card p-4">
                  <div className="mb-2 flex flex-wrap items-center justify-between gap-2">
                    <div>
                      <span className="font-semibold text-foreground">{en.medicalPlanName}</span>
                      <span className={`ml-2 rounded-full px-2 py-0.5 text-xs font-semibold ${statusBadge(en.status)}`}>{t(en.status ?? "")}</span>
                      <span className="ml-2 text-xs text-muted">{en.coverageStart?.slice(0, 10)}{en.coverageEnd ? ` → ${en.coverageEnd.slice(0, 10)}` : ""}</span>
                    </div>
                    <div className="flex items-center gap-1">
                      {en.status === "Active" && <ButtonField value="Suspend" variant="ghost" icon={<PauseCircle size={15} />} onClick={() => en.id && setMedicalEnrollmentStatus(en.id, "Suspended").then((r) => refresh(r.message))} />}
                      {en.status === "Suspended" && <ButtonField value="Reactivate" variant="ghost" icon={<PlayCircle size={15} />} onClick={() => en.id && setMedicalEnrollmentStatus(en.id, "Active").then((r) => refresh(r.message))} />}
                      {en.status !== "Terminated" && <ButtonField value="Terminate" variant="ghost" icon={<Ban size={15} />} onClick={() => en.id && setMedicalEnrollmentStatus(en.id, "Terminated", iso(new Date())).then((r) => refresh(r.message))} />}
                      <ButtonField value="Delete" variant="danger" icon={<Trash2 size={15} />} onClick={() => en.id && deleteMedicalEnrollment(en.id).then((r) => refresh(r.message))} />
                    </div>
                  </div>

                  <DataTableProvider dataTable={{ columns: beneficiaryColumns, data: rows, count: rows.length, pagination: "None", search: "None", key: "id" }} />

                  {en.status !== "Terminated" &&
                    ((dependents?.length ?? 0) === 0 ? (
                      <p className="mt-2 border-t border-border/60 pt-2 text-xs text-muted">
                        {t("No family members on file for this employee. Register them in the employee's Family tab first — this module only enrolls existing family members.")}
                      </p>
                    ) : depOptions.length === 0 ? (
                      <p className="mt-2 border-t border-border/60 pt-2 text-xs text-muted">
                        {t("All family members are already covered by this enrollment.")}
                      </p>
                    ) : (
                      <div className="mt-2 flex flex-wrap items-end gap-2 border-t border-border/60 pt-2">
                        <div className="min-w-56 flex-1">
                          <DropDownField
                            type="dropDown"
                            compact
                            name={`dep-${en.id}`}
                            label="Family member"
                            value={ben[en.id!]?.employeeDependentId ?? ""}
                            displayValue={ben[en.id!]?.dependentName ?? ""}
                            data={depOptions}
                            placeholder={t("Select a family member…")}
                            onSelect={(_n, item) => {
                              const dep = (dependents ?? []).find((d) => d.id === item.id);
                              setBenField(en.id!, { employeeDependentId: item.id, dependentName: item.name, category: toCategory(dep?.relationship) });
                            }}
                          />
                        </div>
                        <div className="w-36">
                          <DropDownField type="dropDown" compact name={`cat-${en.id}`} label="Category" value={ben[en.id!]?.category ?? "Child"} displayValue={ben[en.id!]?.category ?? "Child"} data={CAT_OPTIONS} onSelect={(_n, item) => setBenField(en.id!, { category: item.id })} />
                        </div>
                        <ButtonField value="Add beneficiary" variant="outline" icon={<UserPlus size={14} />} disabled={!ben[en.id!]?.employeeDependentId} onClick={() => addBen(en.id!)} />
                      </div>
                    ))}
                </div>
              );
            })}
            {(enrollments?.length ?? 0) === 0 && <p className="rounded-lg border border-dashed border-border p-6 text-center text-sm text-muted">{t("No enrollments yet.")}</p>}
          </div>
        )}
      </div>
    </EntityModuleShell>
  );
}

export default memo(MedicalEnrollment);
