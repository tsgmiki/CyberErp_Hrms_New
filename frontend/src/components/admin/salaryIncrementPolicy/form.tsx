"use client";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { CalendarClock, ShieldAlert, PieChart, TrendingUp } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import Loading from "@/components/common/loader/loader";
import DetailSection from "@/components/common/detailSection";
import { StatusMessage } from "@/components/common/statusMessage/status";
import type { SalaryIncrementPolicyModel } from "@/models";
import {
  getSalaryIncrementPolicy, saveSalaryIncrementPolicy,
} from "@/services/admin/compensation";

const FormProvider = memo(FormProviders);

/**
 * These MUST mirror the backend's "no policy configured" behaviour, so an unsaved screen shows the
 * rules that are actually running rather than a blank slate that implies nothing is enforced.
 * See SalaryIncrementEligibility: absent a policy the tenure gate is 0 but first-year proration
 * still applies, because paying a full increment for two months' work is the costlier mistake.
 */
const DEFAULTS: SalaryIncrementPolicyModel = {
  name: "Increment eligibility",
  minimumServiceMonths: 0,
  prorateFirstYear: true,
  excludeActiveDisciplinary: true,
  // Off by default, matching the engine: promotion changes an employee's GRADE, not just their pay,
  // so it never starts happening to an existing client without being asked for.
  promoteOnGradeCeiling: false,
  isActive: true,
};

/** Each rule, said once in plain language next to the switch that turns it on. */
const RULES = [
  {
    icon: CalendarClock,
    title: "Minimum service",
    body: "An employee must have completed this many months of service at the revision's effective "
      + "date to qualify. Service is counted in whole completed months, so a 3-month gate means the "
      + "same thing regardless of month lengths. Set 0 for no tenure gate.",
  },
  {
    icon: ShieldAlert,
    title: "Active disciplinary cases",
    body: "Excludes anyone with a disciplinary case that has not been cancelled and has not yet "
      + "expired. This is ANY active case — it does not depend on the case being flagged as blocking "
      + "promotion or reward.",
  },
  {
    icon: PieChart,
    title: "Prorated first year",
    body: "An employee inside their first year earns the increment in proportion to the months they "
      + "have worked: six months in gives half the increase. It scales the increase, never the "
      + "salary, so pay can never go down.",
  },
  {
    icon: TrendingUp,
    title: "Promote at the grade ceiling",
    body: "When a step increment would carry someone past the top of their grade, move them onto the "
      + "next grade up instead of holding them at the ceiling. The next grade is the cheapest one "
      + "that pays more than theirs; one step buys the move and any remainder climbs the new ladder. "
      + "A promotion that would not actually raise pay is refused.",
  },
];

/**
 * The tenant's single active policy. There is no id in the URL and no list: the save endpoint
 * upserts the one active row, so this form always edits "the" policy.
 */
function SalaryIncrementPolicyForm() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState<SalaryIncrementPolicyModel>({ ...DEFAULTS });

  const { data: policy, isLoading } = useQuery({
    queryKey: ["salaryIncrementPolicy"],
    queryFn: getSalaryIncrementPolicy,
  });

  // A null response means "never configured", which is a real state: keep the defaults on screen so
  // the form shows the rules the engine is already applying.
  useEffect(() => {
    if (policy) setFormData({ ...DEFAULTS, ...policy });
  }, [policy]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const toggle = useCallback((name: keyof SalaryIncrementPolicyModel) => (e: any) => {
    setFormData((p) => ({ ...p, [name]: e.target.checked }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    const res = await saveSalaryIncrementPolicy({
      ...formData,
      minimumServiceMonths: Number(formData.minimumServiceMonths ?? 0),
    });
    setIsSaving(false);
    setFormState(res.ok
      ? { status: "success", message: res.message }
      : { status: "error", message: res.message });
    if (res.ok) queryClient.invalidateQueries({ queryKey: ["salaryIncrementPolicy"] });
  };

  if (isLoading) return <Loading />;

  return (
    <div>
      <FormProvider
        ref={formRef}
        form={{
          formId: "salaryIncrementPolicyForm",
          columnsNo: 2,
          labelWidth: "w-[40%]",
          submitHandler,
          isPending: isSaving,
          SubmitButton: "top",
          components: [
            {
              name: "name", label: "Name", required: true, type: "text",
              value: formData.name, onChange: changeHandler,
              error: formState?.zodErrors?.name,
            },
            {
              name: "minimumServiceMonths", label: "Minimum service (months)",
              type: "text", inputType: "number", placeholder: "e.g. 6",
              value: formData.minimumServiceMonths, onChange: changeHandler,
              error: formState?.zodErrors?.minimumServiceMonths,
            },
            {
              name: "excludeActiveDisciplinary", label: "Exclude active disciplinary cases",
              type: "checkbox",
              value: formData.excludeActiveDisciplinary ? "true" : "",
              onChange: toggle("excludeActiveDisciplinary"),
            },
            {
              name: "prorateFirstYear", label: "Prorate the first year",
              type: "checkbox",
              value: formData.prorateFirstYear ? "true" : "",
              onChange: toggle("prorateFirstYear"),
            },
            {
              name: "promoteOnGradeCeiling", label: "Promote at the grade ceiling",
              type: "checkbox",
              value: formData.promoteOnGradeCeiling ? "true" : "",
              onChange: toggle("promoteOnGradeCeiling"),
            },
            {
              name: "isActive", label: "Active",
              type: "checkbox",
              value: formData.isActive ? "true" : "",
              onChange: toggle("isActive"),
            },
          ],
        }}
      />

      {!policy && (
        <p className="mt-2 rounded-md border border-info/30 bg-info/5 px-3 py-2 text-xs text-muted">
          {t("No policy has been saved yet. These are the defaults already in force — save to make them explicit or to change them.")}
        </p>
      )}

      {formData.promoteOnGradeCeiling && (
        <p className="mt-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-warning">
          {t("Applying a revision will now change the JOB GRADE of anyone who clears their ceiling, not just their salary. Grades are sequenced by pay, so an employee moves to the cheapest grade that pays more than their own.")}
        </p>
      )}

      {formData.isActive === false && (
        <p className="mt-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-warning">
          {t("While inactive, no tenure gate applies and every employee receives the full increment — including anyone with an open disciplinary case.")}
        </p>
      )}

      <div className="mt-3">
        <DetailSection title="What these rules do">
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
            {RULES.map(({ icon: Icon, title, body }) => (
              <div key={title} className="rounded-md border border-border bg-secondary/20 p-3">
                <p className="flex items-center gap-2 text-sm font-semibold text-foreground">
                  <Icon size={15} className="shrink-0 text-primary" />
                  {t(title)}
                </p>
                <p className="mt-1 text-xs leading-relaxed text-muted">{t(body)}</p>
              </div>
            ))}
          </div>
          <p className="mt-3 text-xs text-muted">
            {t("Rules are applied when a revision is simulated and again when it is saved: an excluded employee gets no line at all, so they cannot be paid when the revision is applied.")}
          </p>
        </DetailSection>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default SalaryIncrementPolicyForm;
