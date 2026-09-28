"use client";
import React, { memo, useCallback, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Briefcase, Building2, CalendarClock, Coins, GraduationCap, History, Scale, UserCog } from "lucide-react";
import FormProviders from "@/components/common/formProvider/formProvider";
import Loading from "@/components/common/loader/loader";
import { getDelegationPolicy, saveDelegationPolicy } from "@/services/admin/approvalDelegation";
import type { DelegationPolicyModel } from "@/models";

const FormProvider = memo(FormProviders);

/**
 * These MUST mirror the backend's shipped defaults, so an unsaved screen shows the rules actually
 * running rather than a blank slate implying nothing is enforced. See `DelegationPolicy.CreateDefault`.
 */
const DEFAULTS: DelegationPolicyModel = {
  minDelegateExperienceYears: 2,
  minSalaryRatioPercent: 80,
  requireManagerialDelegate: true,
  maxDelegationDays: 90,
  defaultApprovalLimit: null,
  allowSelfServiceDelegation: true,
  restrictToOwnDepartment: true,
  // Off by default, matching the engine: this rule SPENDS MONEY, so it is switched on
  // deliberately rather than inherited by an upgrade.
  actingCompensationEnabled: false,
  actingCompensationMinDays: 90,
  recordActingExperience: true,
};

/** Each rule, said once in plain language beside the control that sets it. */
const RULES = [
  {
    icon: GraduationCap,
    title: "Minimum experience",
    body: "Total years the stand-in must hold — service with this organisation PLUS prior employment "
      + "recorded on their profile. Counting only internal service would rule out an experienced "
      + "senior hire in their first year, who is often exactly the person asked to cover. "
      + "Overlapping past roles are merged, so two concurrent part-time posts over three years "
      + "count as three. Set 0 to drop the rule.",
  },
  {
    icon: Scale,
    title: "Salary parity",
    body: "The stand-in's salary as a percentage of the approver's, at minimum. 80 means somebody "
      + "earning under 80% of the approver cannot hold their authority. This uses salary rather "
      + "than job grade because grades carry a name and a code and no rank, so they cannot be "
      + "ordered. If either salary is missing the rule cannot be checked and the delegation is "
      + "refused rather than waved through. Set 0 to drop the rule.",
  },
  {
    icon: UserCog,
    title: "Managerial parity",
    body: "When the approver holds a managerial post, the stand-in must hold one too. Keys off the "
      + "employee record's managerial flag, so it is only as reliable as that flag.",
  },
  {
    icon: Building2,
    title: "Own department only",
    body: "Confines the stand-in to the approver's own department and the departments beneath it — "
      + "a head covers their own area. Switch it off for a flat organisation, or one that covers "
      + "across sites: it widens WHO may be chosen, never what they may do. HR is exempt either way.",
  },
  {
    icon: CalendarClock,
    title: "Longest delegation",
    body: "The most days a single delegation may run. An indefinite delegation is not a delegation, "
      + "it is an undocumented change to the approval chain that nobody revisits; a bounded window "
      + "forces the question to be asked again. Set 0 for no limit.",
  },
  {
    icon: Briefcase,
    title: "Acting compensation",
    body: "Past the threshold, standing in stops being a favour and becomes a job: the deputy is "
      + "paid the rate of the POST they are covering — taken from that position's salary scale, not "
      + "from what its current holder personally earns — for the length of the assignment. It is "
      + "raised for approval, never applied automatically, and the deputy reverts to their own "
      + "salary when the cover ends. An acting rate that would not exceed what they already earn "
      + "raises nothing.",
  },
  {
    icon: History,
    title: "Acting counts as experience",
    body: "On conclusion, the period is written into the deputy's experience record as internal "
      + "service. That feeds straight back into the rules above: covering a post once helps qualify "
      + "somebody to cover one again.",
  },
  {
    icon: Coins,
    title: "Default approval ceiling",
    body: "Applied when a delegation names no ceiling of its own. A request above the ceiling is "
      + "never rejected — it simply stays with the real approver, because a stand-in who cannot "
      + "sign for the amount is a reason to wait, not a reason to refuse somebody's loan. Leave "
      + "blank for no default ceiling.",
  },
];

/**
 * The tenant's single delegation policy.
 *
 * <p>⚠️ Three delegation guards are deliberately ABSENT from this screen and cannot be switched
 * off: authority received through a delegation cannot be delegated onward, a stand-in can never
 * approve their own request, and an unassigned workflow step confers nothing. Those are invariants
 * rather than preferences — an organisation that turned any of them off would have an approval
 * chain that does not mean anything, and offering the switch implies it is a reasonable thing to
 * want. They are stated at the foot of the page so their absence is visible, not silent.</p>
 */
function DelegationPolicyForm() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const formRef = React.createRef<HTMLFormElement>();
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);
  const [formData, setFormData] = useState<DelegationPolicyModel>({ ...DEFAULTS });

  const { data: policy, isLoading } = useQuery({
    queryKey: ["delegationPolicy"],
    queryFn: getDelegationPolicy,
  });

  useEffect(() => {
    if (policy) setFormData({ ...DEFAULTS, ...policy });
  }, [policy]);

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);

  const toggle = useCallback(
    (name: keyof DelegationPolicyModel) => (e: any) => {
      setFormData((p) => ({ ...p, [name]: e.target.checked }));
    },
    [],
  );

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsSaving(true);
    try {
      const res = await saveDelegationPolicy({
        ...formData,
        minDelegateExperienceYears: Number(formData.minDelegateExperienceYears ?? 0),
        minSalaryRatioPercent: Number(formData.minSalaryRatioPercent ?? 0),
        maxDelegationDays: Number(formData.maxDelegationDays ?? 0),
        actingCompensationMinDays: Number(formData.actingCompensationMinDays ?? 90),
        // Blank means "no default ceiling", which is not a ceiling of zero.
        defaultApprovalLimit:
          formData.defaultApprovalLimit === null ||
          formData.defaultApprovalLimit === undefined ||
          (formData.defaultApprovalLimit as unknown) === ""
            ? null
            : Number(formData.defaultApprovalLimit),
      });
      setFormState({ status: "success", message: res?.message ?? t("Delegation rules saved") });
      queryClient.invalidateQueries({ queryKey: ["delegationPolicy"] });
      // Any open delegation form is judging against these rules — make it re-ask.
      queryClient.invalidateQueries({ queryKey: ["delegationEligibility"] });
    } catch (err) {
      setFormState({
        status: "error",
        message: err instanceof Error ? err.message : t("Could not save the delegation rules."),
      });
    }
    setIsSaving(false);
  };

  if (isLoading) return <Loading />;

  return (
    <div>
      {formState.status && (
        <p
          className={`mb-3 rounded-md border px-3 py-2 text-xs ${
            formState.status === "success"
              ? "border-success/20 bg-success/15 text-success"
              : "border-error/20 bg-error/15 text-error"
          }`}
        >
          {formState.message}
        </p>
      )}

      <FormProvider
        ref={formRef}
        form={{
          formId: "delegationPolicyForm",
          columnsNo: 2,
          labelWidth: "w-[45%]",
          submitHandler,
          isPending: isSaving,
          SubmitButton: "top",
          components: [
            {
              name: "minDelegateExperienceYears", label: "Minimum experience (years)",
              type: "text", inputType: "number", placeholder: "e.g. 2",
              value: formData.minDelegateExperienceYears, onChange: changeHandler,
            },
            {
              name: "minSalaryRatioPercent", label: "Minimum salary parity (%)",
              type: "text", inputType: "number", placeholder: "e.g. 80",
              value: formData.minSalaryRatioPercent, onChange: changeHandler,
            },
            {
              name: "maxDelegationDays", label: "Longest delegation (days)",
              type: "text", inputType: "number", placeholder: "e.g. 90",
              value: formData.maxDelegationDays, onChange: changeHandler,
            },
            {
              name: "defaultApprovalLimit", label: "Default approval ceiling",
              type: "text", inputType: "number", placeholder: "Blank = no ceiling",
              value: formData.defaultApprovalLimit ?? "", onChange: changeHandler,
            },
            {
              name: "requireManagerialDelegate", label: "Require a managerial stand-in",
              type: "checkbox",
              value: formData.requireManagerialDelegate ? "true" : "",
              onChange: toggle("requireManagerialDelegate"),
            },
            {
              name: "restrictToOwnDepartment", label: "Own department only",
              type: "checkbox",
              value: formData.restrictToOwnDepartment ? "true" : "",
              onChange: toggle("restrictToOwnDepartment"),
            },
            {
              name: "allowSelfServiceDelegation", label: "Let approvers arrange their own cover",
              type: "checkbox",
              value: formData.allowSelfServiceDelegation ? "true" : "",
              onChange: toggle("allowSelfServiceDelegation"),
            },
            {
              name: "actingCompensationEnabled", label: "Pay the deputy for long cover",
              type: "checkbox",
              value: formData.actingCompensationEnabled ? "true" : "",
              onChange: toggle("actingCompensationEnabled"),
            },
            {
              name: "actingCompensationMinDays", label: "Acting threshold (days)",
              type: "text", inputType: "number", placeholder: "e.g. 90",
              value: formData.actingCompensationMinDays, onChange: changeHandler,
            },
            {
              name: "recordActingExperience", label: "Count acting as experience",
              type: "checkbox",
              value: formData.recordActingExperience ? "true" : "",
              onChange: toggle("recordActingExperience"),
            },
          ],
        }}
      />

      {!policy && (
        <p className="mt-2 rounded-md border border-info/20 bg-info/15 px-3 py-2 text-xs text-muted">
          {t("No policy has been saved yet. These are the defaults already in force — save to make them explicit or to change them.")}
        </p>
      )}

      {/* ⚠️ THE TRAP THESE TWO NUMBERS SET FOR EACH OTHER. A delegation may run at most
          maxDelegationDays, and must EXCEED actingCompensationMinDays to earn acting pay. On the
          shipped defaults both are 90, so no delegation can ever qualify — the feature would be
          switched on, look configured, and do nothing at all, with no error anywhere to explain
          why. Said here, where both numbers are on screen together. */}
      {formData.actingCompensationEnabled &&
        Number(formData.maxDelegationDays) > 0 &&
        Number(formData.maxDelegationDays) <= Number(formData.actingCompensationMinDays) && (
          <p className="mt-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-warning">
            {t("No delegation can qualify for acting pay: the longest one allowed is")}{" "}
            {formData.maxDelegationDays} {t("days, and the acting threshold is")}{" "}
            {formData.actingCompensationMinDays}{" "}
            {t("days — a delegation must EXCEED the threshold. Raise the longest delegation, or lower the threshold.")}
          </p>
        )}

      {formData.actingCompensationEnabled && (
        <p className="mt-2 rounded-md border border-info/20 bg-info/15 px-3 py-2 text-xs text-muted">
          {t("Qualifying delegations will raise an acting-pay assignment for approval. Nothing changes anybody's salary until it is approved, and it needs an active 'ActingAssignment' workflow — without one the assignment stays pending.")}
        </p>
      )}

      {formData.restrictToOwnDepartment === false && (
        <p className="mt-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-warning">
          {t("Approvers may now name a stand-in anywhere in the organisation, not only their own department. The experience, salary and managerial rules above still apply.")}
        </p>
      )}

      {formData.allowSelfServiceDelegation === false && (
        <p className="mt-2 rounded-md border border-warning/30 bg-warning/10 px-3 py-2 text-xs text-warning">
          {t("Only HR can now arrange cover. Department heads will not be able to delegate their own approvals before going on leave.")}
        </p>
      )}

      {/* What each rule means, next to the screen that sets it. */}
      <div className="mt-4 grid gap-2 md:grid-cols-2">
        {RULES.map(({ icon: Icon, title, body }) => (
          <div key={title} className="rounded-lg border border-border bg-card p-3">
            <p className="flex items-center gap-1.5 text-xs font-semibold text-foreground">
              <Icon size={13} className="shrink-0 text-primary" /> {t(title)}
            </p>
            <p className="mt-1 text-[11px] leading-relaxed text-muted">{t(body)}</p>
          </div>
        ))}
      </div>

      {/* ⚠️ Stated so their absence is visible rather than silent. */}
      <div className="mt-3 rounded-lg border border-border bg-secondary/20 p-3">
        <p className="text-xs font-semibold text-foreground">{t("Always enforced, and not configurable")}</p>
        <ul className="mt-1 list-disc space-y-0.5 pl-4 text-[11px] leading-relaxed text-muted">
          <li>{t("Authority received through a delegation cannot be delegated onward — a chain would move approval rights to somebody nobody chose.")}</li>
          <li>{t("A stand-in can never approve their own request, whatever they are covering.")}</li>
          <li>{t("A workflow step with no assigned approvers confers nothing — there is no authority there to lend.")}</li>
          <li>{t("HR may always arrange cover for anyone, and is exempt from the department rule.")}</li>
        </ul>
      </div>
    </div>
  );
}

export default DelegationPolicyForm;
