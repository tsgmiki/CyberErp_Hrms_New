"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { Plus, Play, Ban, Pencil, Trash, ArrowRight } from "lucide-react";
import type { EmployeeMovementModel } from "@/models";
import {
  getMovements,
  saveMovement,
  deleteMovement,
  executeMovement,
  cancelMovement,
} from "@/services/admin/employee/personnelActions";
import getAllPosition from "@/services/admin/position/getAll";
import getAllJobGrade from "@/services/admin/jobGrade/getAll";
import getAllSalaryScale from "@/services/admin/salaryScale/getAll";
import Loading from "../../common/loader/loader";
import { StatusMessage } from "../../common/statusMessage/status";
import { useCustomFields } from "./customFieldsHook";
import { parameterInitialData } from "@/constants/initialization";
import { movementTypeOptions } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const fmtDate = (v?: string) => (v ? v.slice(0, 10) : "");

const STATUS_TONE: Record<string, string> = {
  Pending: "bg-warning/15 text-warning",
  Completed: "bg-success/15 text-success",
  Cancelled: "bg-muted/30 text-muted",
};

/** from → to summary of what the movement changes. */
function MovementChange({ m }: { m: EmployeeMovementModel }) {
  const parts: { from?: string | number; to?: string | number }[] = [];
  if (m.toPositionName) parts.push({ from: m.fromPositionName ?? "—", to: m.toPositionName });
  if (m.toSalaryScaleName) parts.push({ from: m.fromSalaryScaleName ?? "—", to: m.toSalaryScaleName });
  if (m.toSalary != null) parts.push({ from: m.fromSalary ?? "—", to: m.toSalary });
  if (parts.length === 0) return <span className="text-muted">—</span>;
  return (
    <span className="space-y-0.5">
      {parts.map((p, i) => (
        <span key={i} className="flex items-center gap-1 text-xs">
          <span className="text-muted">{p.from}</span>
          <ArrowRight size={11} className="shrink-0 text-muted" />
          <span className="font-medium text-foreground">{p.to}</span>
        </span>
      ))}
    </span>
  );
}

function MovementSection({ employeeId }: { employeeId: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<EmployeeMovementModel | null>(null);
  const [formState, setFormState] = useState<any>({});
  const [formData, setFormData] = useState<EmployeeMovementModel>({});
  const [isSaving, setIsSaving] = useState(false);
  const customFields = useCustomFields("Movement");

  const queryKey = ["employeeMovements", employeeId];
  const { data: rows, isLoading } = useQuery({
    queryKey,
    queryFn: () => getMovements(employeeId),
  });

  // Target position lookup — vacant positions only (the employee moves into an open seat).
  const [positionParam, setPositionParam] = useState({
    ...parameterInitialData,
    take: 100,
    isVacant: true,
  });
  const { data: positions, isLoading: positionsLoading } = useQuery({
    queryKey: ["positions", positionParam],
    queryFn: () => getAllPosition(positionParam),
  });

  const [gradeParam, setGradeParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: grades, isLoading: gradesLoading } = useQuery({
    queryKey: ["jobGrades", gradeParam],
    queryFn: () => getAllJobGrade(gradeParam),
  });

  // Salary scales are scoped to the selected job grade (mirrors the employee master form).
  const [scaleParam, setScaleParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: scales, isLoading: scalesLoading } = useQuery({
    queryKey: ["salaryScales", "byGrade", formData.jobGradeId, scaleParam],
    queryFn: () => getAllSalaryScale({ ...scaleParam, jobGradeId: formData.jobGradeId }),
    enabled: !!formData.jobGradeId,
  });

  const refresh = useCallback(() => {
    queryClient.invalidateQueries({ queryKey });
    // An executed movement changes the employee master + position vacancy.
    queryClient.invalidateQueries({ queryKey: ["employees"] });
    queryClient.invalidateQueries({ queryKey: ["employee", employeeId] });
    queryClient.invalidateQueries({ queryKey: ["positions"] });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [queryClient, employeeId]);

  const { mutate: remove } = useMutation({
    mutationFn: (id: string) => deleteMovement(id),
    onSuccess: (r: any) => {
      if (r?.status === "error") return setError(r.message);
      setError(null);
      refresh();
    },
  });

  const runAction = async (fn: (id: string) => Promise<{ ok: boolean; message: string }>, id: string) => {
    const res = await fn(id);
    if (!res.ok) setError(res.message);
    else setError(null);
    refresh();
  };

  const open = (record: EmployeeMovementModel | null) => {
    setEditing(record);
    setFormData(
      record
        ? { ...record, effectiveDate: fmtDate(record.effectiveDate) }
        : { movementType: "Transfer" },
    );
    customFields.hydrate(record?.customFields);
    setFormState({});
    setShowForm(true);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);
  // Choosing a job grade re-scopes the salary scale list; the previous scale/salary is cleared.
  const gradeSelectHandler = useCallback((_name: string, r: any) => {
    setFormData((p) => ({ ...p, jobGradeId: r.id, jobGradeName: r.name, toSalaryScaleId: undefined, toSalaryScaleName: undefined }));
  }, []);
  // Picking a salary scale records it and auto-fills the (still editable) new salary.
  const scaleSelectHandler = useCallback(
    (_name: string, r: any) => {
      const scale = (scales?.data ?? []).find((s) => s.id === r.id);
      setFormData((p) => ({
        ...p,
        toSalaryScaleId: r.id,
        toSalaryScaleName: scale?.step,
        toSalary: scale?.salary ?? p.toSalary,
      }));
    },
    [scales],
  );

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsSaving(true);
    const result = await saveMovement(fd);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      refresh();
      setShowForm(false);
    }
  };

  const actionBtn =
    "rounded p-1 transition-colors disabled:cursor-not-allowed disabled:opacity-40";

  return (
    <>
      <div className="m-1 rounded-lg border border-border bg-card">
        <div className="flex items-center justify-between border-b border-border px-4 py-2.5">
          <h3 className="text-sm font-semibold text-foreground">
            {t("Movements (Transfers, Promotions & Demotions)")}
          </h3>
          <button
            type="button"
            onClick={() => open(null)}
            className="flex items-center gap-1 rounded bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90"
          >
            <Plus className="h-3.5 w-3.5" /> {t("Record Movement")}
          </button>
        </div>

        {error && (
          <div className="mx-4 mt-2 flex items-center justify-between rounded border border-error/30 bg-error/15 px-3 py-2 text-xs text-error">
            <span>{error}</span>
            <button type="button" onClick={() => setError(null)} className="font-semibold">×</button>
          </div>
        )}

        {isLoading ? (
          <Loading />
        ) : (rows?.length ?? 0) === 0 ? (
          <p className="px-4 py-8 text-center text-sm text-muted">
            {t("No records yet. Use the add button above.")}
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="px-4 py-2 font-semibold">{t("Type")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Effective Date")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Change")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Reason")}</th>
                  <th className="px-4 py-2 font-semibold">{t("Status")}</th>
                  <th className="px-4 py-2 text-right font-semibold">{t("Action")}</th>
                </tr>
              </thead>
              <tbody>
                {rows!.map((m) => {
                  const pending = m.status === "Pending";
                  return (
                    <tr key={m.id} className="border-b border-border/60 hover:bg-secondary/40">
                      <td className="px-4 py-2.5 font-medium text-foreground">{t(m.movementType ?? "")}</td>
                      <td className="px-4 py-2.5 text-foreground">{fmtDate(m.effectiveDate)}</td>
                      <td className="px-4 py-2.5"><MovementChange m={m} /></td>
                      <td className="max-w-[220px] truncate px-4 py-2.5 text-muted" title={m.reason}>
                        {m.reason || "—"}
                      </td>
                      <td className="px-4 py-2.5">
                        <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[m.status ?? ""] ?? "bg-muted/30 text-muted"}`}>
                          {t(m.status ?? "")}
                        </span>
                      </td>
                      <td className="px-4 py-1.5 text-right">
                        <span className="inline-flex items-center gap-0.5">
                          <button
                            type="button" title={t("Execute")} disabled={!pending}
                            onClick={() => m.id && runAction(executeMovement, m.id)}
                            className={`${actionBtn} text-success hover:bg-success/10`}
                          ><Play size={15} /></button>
                          <button
                            type="button" title={t("Cancel")} disabled={!pending}
                            onClick={() => m.id && runAction(cancelMovement, m.id)}
                            className={`${actionBtn} text-warning hover:bg-warning/10`}
                          ><Ban size={15} /></button>
                          <button
                            type="button" title={t("Edit")} disabled={!pending}
                            onClick={() => open(m)}
                            className={`${actionBtn} text-primary hover:bg-primary/10`}
                          ><Pencil size={15} /></button>
                          <button
                            type="button" title={t("Delete")} disabled={m.status === "Completed"}
                            onClick={() => m.id && remove(m.id)}
                            className={`${actionBtn} text-error hover:bg-error/10`}
                          ><Trash size={15} /></button>
                        </span>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {showForm && (
        <FormProvider
          form={{
            columnsNo: 2,
            submitHandler,
            fieldLayout: "auth",
            isPending: isSaving,
            SubmitButton: "top",
            showModal: true,
            modalVisible: true,
            modalTitle: editing ? "Edit Movement" : "Record Movement",
            description: "Transfer, promotion or demotion.",
            modalSize: "lg",
            onModalClose: () => setShowForm(false),
            submitBtnTitle: "Save",
            components: [
              {
                name: "movementType", label: "Movement Type", required: true, type: "dropDown",
                onSelect: selectHandler, value: formData.movementType, displayValue: formData.movementType,
                error: formState?.zodErrors?.movementType, data: movementTypeOptions as never,
              },
              {
                name: "effectiveDate", label: "Effective Date", required: true, type: "date",
                value: formData.effectiveDate, onChange: changeHandler,
                error: formState?.zodErrors?.effectiveDate,
              },
              {
                name: "toPositionId",
                label: formData.movementType === "Transfer" ? "Target Position" : "Target Position (optional)",
                required: formData.movementType === "Transfer",
                type: "dropDown", onSelect: selectHandler,
                value: formData.toPositionId, displayValue: formData.toPositionName,
                param: positionParam, setParam: setPositionParam as any, isLoading: positionsLoading,
                data: (positions?.data ?? []).map((p) => ({
                  id: p.id,
                  name: `${p.code} — ${p.positionClassTitle ?? ""}${p.organizationUnitName ? ` · ${p.organizationUnitName}` : ""}`,
                })) as never,
              },
              // Pay change (grade → scale → salary) is available ONLY for a Promotion or Demotion —
              // a Transfer moves the employee without changing compensation (backend enforces this).
              ...(formData.movementType !== "Transfer"
                ? [
                    {
                      name: "jobGradeId", label: "Job Grade (filter)", type: "dropDown" as const, onSelect: gradeSelectHandler,
                      value: formData.jobGradeId, displayValue: formData.jobGradeName,
                      placeholder: "Filter salary scales by grade",
                      param: gradeParam, setParam: setGradeParam as any, isLoading: gradesLoading,
                      data: (grades?.data ?? []).map((g) => ({ id: g.id, name: g.name })) as never,
                    },
                    {
                      name: "toSalaryScaleId", label: "New Salary Scale (Step)", type: "dropDown" as const, onSelect: scaleSelectHandler,
                      value: formData.toSalaryScaleId, displayValue: formData.toSalaryScaleName,
                      disabled: !formData.jobGradeId,
                      placeholder: formData.jobGradeId ? "Select a step" : "Select a job grade first",
                      param: scaleParam, setParam: setScaleParam as any, isLoading: scalesLoading,
                      data: (scales?.data ?? []).map((s) => ({
                        id: s.id,
                        name: `${s.step ?? "Step"} — ${s.salary != null ? Number(s.salary).toLocaleString(undefined, { minimumFractionDigits: 2 }) : ""}`,
                      })) as never,
                    },
                    {
                      name: "toSalary", label: "New Salary (optional)", value: formData.toSalary,
                      onChange: changeHandler, inputType: "number" as const, type: "text" as const,
                    },
                  ]
                : []),
              { name: "reason", label: "Reason", value: formData.reason, onChange: changeHandler, type: "textarea", colSpan: "full" },
              { name: "remark", label: "Remark", value: formData.remark, onChange: changeHandler, type: "textarea", colSpan: "full" },
              ...customFields.components,
              { name: "employeeId", value: employeeId, type: "hidden" },
              { name: "id", value: formData.id, type: "hidden" },
            ],
          }}
        >
          <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
        </FormProvider>
      )}
    </>
  );
}

export default MovementSection;
