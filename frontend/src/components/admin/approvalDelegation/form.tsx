"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import React, { memo, useCallback, useEffect, useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { AlertTriangle, CheckCircle2, ShieldCheck } from "lucide-react";
import EmployeePicker from "@/components/common/employeePicker";
import { StatusMessage } from "@/components/common/statusMessage/status";
import {
  saveApprovalDelegation,
  checkDelegationEligibility,
  getDelegationPolicy,
} from "@/services/admin/approvalDelegation";
import { workflowEntityTypeOptions } from "@/constants/orgStructure";
import type { ApprovalDelegationModel } from "@/models";

const FormProvider = memo(FormProviders);

const today = () => {
  const n = new Date();
  return new Date(n.getTime() - n.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
};

const EMPTY: ApprovalDelegationModel = { allProcesses: true, entityTypes: [] };

/**
 * Create or amend one delegation.
 *
 * <p>⚠️ The seniority rules are checked LIVE, as the two employees are chosen, rather than only at
 * save. A rule that speaks for the first time when you press Save reads as the system being
 * obstructive; the same rule shown while you pick reads as the policy it is — and it names the
 * figures it judged on, so the answer can be argued with.</p>
 */
function ApprovalDelegationForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState<ApprovalDelegationModel>({ ...EMPTY });
  const formRef = React.createRef<HTMLFormElement>();

  // Stale-form guard: dropping the id while this form stays mounted must not leave the previous
  // record's values behind for the next Add.
  useEffect(() => {
    if (!id) setFormData({ ...EMPTY });
  }, [id]);

  const { data: policy } = useQuery({
    queryKey: ["delegationPolicy"],
    queryFn: getDelegationPolicy,
  });

  // The live verdict. Only asked once BOTH employees are chosen and they differ — an eligibility
  // answer about a half-filled form is noise.
  const canCheck =
    !!formData.fromEmployeeId && !!formData.toEmployeeId &&
    formData.fromEmployeeId !== formData.toEmployeeId;
  const { data: eligibility, isFetching: checking } = useQuery({
    queryKey: ["delegationEligibility", formData.fromEmployeeId, formData.toEmployeeId],
    queryFn: () => checkDelegationEligibility(formData.fromEmployeeId!, formData.toEmployeeId!),
    enabled: canCheck,
  });

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const toggleProcess = (key: string) =>
    setFormData((p) => {
      const current = p.entityTypes ?? [];
      return {
        ...p,
        entityTypes: current.includes(key) ? current.filter((k) => k !== key) : [...current, key],
      };
    });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await saveApprovalDelegation({ ...formData, id: formData.id || id || undefined });
    setFormState(result);
    setIsSaving(false);
  };

  useEffect(() => {
    if (formState.status === "success") {
      setFormData({ ...EMPTY });
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["approvalDelegations"] });
      setId("");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div className="text-foreground">
      <StatusMessage status={formState?.status} message={formState?.message} formState={formState} />

      {/* Who lends, and who stands in. Both go through the role-scoped EmployeePicker, so the
          server decides which employees the caller may even see. */}
      <div className="mb-3 grid grid-cols-1 gap-3 md:grid-cols-2">
        <div>
          <label className="mb-1 block text-xs font-medium text-muted">
            {t("Approver (whose authority is lent)")} *
          </label>
          <EmployeePicker
            value={formData.fromEmployeeId}
            displayValue={formData.fromEmployeeName}
            onSelect={(eid, name) =>
              setFormData((p) => ({ ...p, fromEmployeeId: eid, fromEmployeeName: name }))
            }
            placeholder={t("Search employees") ?? undefined}
          />
          {formState?.zodErrors?.fromEmployeeId && (
            <p className="mt-1 text-[11px] text-error">{formState.zodErrors.fromEmployeeId[0]}</p>
          )}
        </div>
        <div>
          <label className="mb-1 block text-xs font-medium text-muted">{t("Stand-in")} *</label>
          <EmployeePicker
            value={formData.toEmployeeId}
            displayValue={formData.toEmployeeName}
            onSelect={(eid, name) =>
              setFormData((p) => ({ ...p, toEmployeeId: eid, toEmployeeName: name }))
            }
            excludeId={formData.fromEmployeeId}
            placeholder={t("Search employees") ?? undefined}
          />
          {formState?.zodErrors?.toEmployeeId && (
            <p className="mt-1 text-[11px] text-error">{formState.zodErrors.toEmployeeId[0]}</p>
          )}
        </div>
      </div>

      {/* The seniority verdict, with the figures it judged on. */}
      {canCheck && (
        <div
          className={`mb-3 rounded-md border px-3 py-2 text-xs ${
            checking
              ? "border-border bg-secondary/20 text-muted"
              : eligibility?.isEligible
                ? "border-success/20 bg-success/15 text-success"
                : "border-warning/30 bg-warning/10 text-warning"
          }`}
        >
          {checking ? (
            <span>{t("Checking eligibility")}…</span>
          ) : (
            <>
              <p className="flex items-center gap-1.5 font-semibold">
                {eligibility?.isEligible ? <CheckCircle2 size={13} /> : <AlertTriangle size={13} />}
                {eligibility?.isEligible
                  ? t("This delegate meets the seniority rules.")
                  : t("This delegate does not meet the seniority rules.")}
              </p>
              {(eligibility?.reasons ?? []).map((r, i) => (
                <p key={i} className="mt-1">{r}</p>
              ))}
              <p className="mt-1 text-[11px] opacity-80">
                {t("Experience")}: {eligibility?.delegateExperienceYears ?? 0} {t("yr(s)")}
                {eligibility?.salaryRatioPercent != null && (
                  <> · {t("salary")} {eligibility.salaryRatioPercent}% {t("of the approver's")}</>
                )}
                {policy && (
                  <> · {t("policy requires")} {policy.minDelegateExperienceYears} {t("yr(s)")} {t("and")} {policy.minSalaryRatioPercent}%</>
                )}
              </p>
            </>
          )}
        </div>
      )}

      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isSaving,
          SubmitButton: "top",
          components: [
            {
              name: "startDate", label: "From", type: "date", required: true,
              value: formData.startDate ?? today(), onChange: changeHandler,
              error: formState?.zodErrors?.startDate,
            },
            {
              name: "endDate", label: "To", type: "date", required: true,
              value: formData.endDate, onChange: changeHandler,
              error: formState?.zodErrors?.endDate,
            },
            {
              name: "approvalLimit", label: "Approval ceiling", type: "text",
              placeholder: policy?.maxDelegationDays
                ? `${t("Blank = no limit")}`
                : t("Blank = no limit") ?? "",
              value: formData.approvalLimit ?? "", onChange: changeHandler,
            },
            {
              name: "reason", label: "Reason", type: "textarea",
              placeholder: "e.g. Annual leave cover",
              value: formData.reason, onChange: changeHandler,
            },
          ],
        }}
      />

      {/* Scope. All-processes is the default because it is the honest common case — somebody is
          away and everything they approve needs covering. */}
      <div className="mt-4 rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <div>
            <h4 className="flex items-center gap-1.5 text-sm font-semibold text-foreground">
              <ShieldCheck size={14} className="text-primary" /> {t("What this covers")}
            </h4>
            <p className="text-xs text-muted">
              {t("A request above the ceiling is not rejected — it simply stays with the real approver.")}
            </p>
          </div>
          <label className="flex cursor-pointer items-center gap-2 text-xs">
            <input
              type="checkbox"
              className="h-3.5 w-3.5 accent-[var(--primary)]"
              checked={formData.allProcesses !== false}
              onChange={(e) => setFormData((p) => ({ ...p, allProcesses: e.target.checked }))}
            />
            {t("All processes")}
          </label>
        </div>

        {formData.allProcesses === false && (
          <>
            <div className="flex flex-wrap gap-1.5">
              {workflowEntityTypeOptions.map((o) => {
                const on = (formData.entityTypes ?? []).includes(o.id);
                return (
                  <button
                    key={o.id}
                    type="button"
                    onClick={() => toggleProcess(o.id)}
                    className={`rounded-full border px-2.5 py-1 text-[11px] transition-colors ${
                      on
                        ? "border-primary bg-primary/15 font-semibold text-primary"
                        : "border-border text-muted hover:text-foreground"
                    }`}
                  >
                    {t(o.name)}
                  </button>
                );
              })}
            </div>
            {formState?.zodErrors?.entityTypes && (
              <p className="mt-2 text-[11px] text-error">{formState.zodErrors.entityTypes[0]}</p>
            )}
          </>
        )}
      </div>
    </div>
  );
}

export default ApprovalDelegationForm;
