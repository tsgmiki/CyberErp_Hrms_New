"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, ShieldAlert, Ban, Award, TrendingUp } from "lucide-react";
import type { DisciplinaryMeasureModel } from "@/models";
import { getDisciplinaryCase, saveDisciplinaryCase } from "@/services/admin/disciplinaryCase";
import EmployeePicker from "@/components/common/employeePicker";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { measureTypeOptions, disciplinaryStatusOptions } from "@/constants/orgStructure";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

// affectsSalaryIncrement starts TRUE: it is an opt-OUT (see the model), so a new case blocks an
// increment unless HR unticks it.
const NEW_DEFAULTS: DisciplinaryMeasureModel = {
  measureType: "VerbalWarning", status: "Open", affectsSalaryIncrement: true,
};

function DisciplinaryCaseForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<DisciplinaryMeasureModel>({ ...NEW_DEFAULTS });
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["disciplinaryCase", id],
    queryFn: () => getDisciplinaryCase(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record)
      setMeta({
        ...record,
        violationDate: record.violationDate?.slice(0, 10),
        effectiveDate: record.effectiveDate?.slice(0, 10),
        validUntil: record.validUntil?.slice(0, 10),
      });
  }, [record]);
  // stale-form guard: when the id is cleared (back / Add-new) while this form stays
  // mounted, drop the previously loaded record so Add never shows stale values.
  useEffect(() => {
    if (!id) {
      setMeta({ ...NEW_DEFAULTS });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);


  const set = (name: keyof DisciplinaryMeasureModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  // A case under an active approval workflow can't be edited (server gate); Cancelled/edits allowed otherwise.
  const editable = !id || meta.status !== "UnderReview";

  const submit = async () => {
    setIsSaving(true);
    const result = await saveDisciplinaryCase({
      id: meta.id || id || undefined,
      employeeId: meta.employeeId,
      violationDate: meta.violationDate,
      violationType: meta.violationType,
      measureType: meta.measureType,
      status: meta.status,
      description: meta.description,
      effectiveDate: meta.effectiveDate || undefined,
      resolution: meta.resolution,
      validUntil: meta.validUntil || undefined,
      affectsPromotion: !!meta.affectsPromotion,
      affectsReward: !!meta.affectsReward,
      // `!== false`, not `!!`: a record loaded before this field existed must still count as blocking.
      affectsSalaryIncrement: meta.affectsSalaryIncrement !== false,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["disciplinaryCases"] });
      // The record's OWN cache entry, not just the list: without this the detail query
      // ["disciplinaryCase", id] kept the pre-save copy and the client's 30 s staleTime served it to the
      // next Edit WITHOUT refetching -- grid fresh, form stale until a full page reload.
      queryClient.invalidateQueries({ queryKey: ["disciplinaryCase"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = editable && !!meta.employeeId && !!meta.violationDate && !!meta.violationType && !!meta.measureType;

  return (
    <div className="space-y-4 text-foreground">
      {id && meta.raisedByName && (
        <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("Raised by")} <span className="font-semibold text-foreground">{meta.raisedByName}</span>
        </p>
      )}

      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <ShieldAlert size={16} className="text-primary" /> {t("Disciplinary Case")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Employee")} *</label>
            {/* Role-scoped: HR = all, manager = unit subtree; never yourself (server-enforced, HC222). */}
            <EmployeePicker
              value={meta.employeeId}
              displayValue={meta.employeeName}
              disabled={!editable || !!id}
              onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
            />
          </div>
          <div>
            <label className={LABEL}>{t("Violation Type")} *</label>
            <input type="text" className={INPUT} disabled={!editable} value={meta.violationType ?? ""}
              onChange={(e) => set("violationType", e.target.value)} placeholder={t("e.g. Absenteeism, Misconduct") ?? ""} />
          </div>
          <div>
            <label className={LABEL}>{t("Violation Date")} *</label>
            <input type="date" className={INPUT} disabled={!editable} value={meta.violationDate ?? ""}
              onChange={(e) => set("violationDate", e.target.value)} />
          </div>
          <div>
            <label className={LABEL}>{t("Measure")} *</label>
            <select className={INPUT} disabled={!editable} value={meta.measureType ?? ""} onChange={(e) => set("measureType", e.target.value)}>
              {measureTypeOptions.map((o) => <option key={o.id} value={o.id}>{t(o.name)}</option>)}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Status")}</label>
            <select className={INPUT} disabled={!editable} value={meta.status ?? ""} onChange={(e) => set("status", e.target.value)}>
              {disciplinaryStatusOptions.map((o) => <option key={o.id} value={o.id}>{t(o.name)}</option>)}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Effective Date")}</label>
            <input type="date" className={INPUT} disabled={!editable} value={meta.effectiveDate ?? ""}
              onChange={(e) => set("effectiveDate", e.target.value)} />
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Description")}</label>
            <textarea rows={2} className={INPUT} disabled={!editable} value={meta.description ?? ""}
              onChange={(e) => set("description", e.target.value)} />
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Resolution")}</label>
            <textarea rows={2} className={INPUT} disabled={!editable} value={meta.resolution ?? ""}
              onChange={(e) => set("resolution", e.target.value)} />
          </div>
        </div>
      </div>

      {/* HC223/HC225 — lifetime + eligibility impact. Promotion and reward are opt-IN; the salary
          increment is opt-OUT (ticked by default), because every active case blocked an increment
          before that flag existed. */}
      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-1 flex items-center gap-2 text-sm font-semibold">
          <Ban size={16} className="text-primary" /> {t("Lifetime & Eligibility Impact")}
        </h3>
        <p className="mb-3 text-xs text-muted">
          {t("While active (until the lifetime date), a ticked measure blocks the chosen actions. Salary increment is ticked by default — untick it to let this case keep its increment. Leave the date blank for an open-ended measure.")}
        </p>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Valid Until (lifetime)")}</label>
            <input type="date" className={INPUT} disabled={!editable} value={meta.validUntil ?? ""}
              onChange={(e) => set("validUntil", e.target.value)} />
          </div>
          <div className="flex items-end gap-4 pb-1.5">
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input type="checkbox" disabled={!editable} checked={!!meta.affectsPromotion}
                onChange={(e) => set("affectsPromotion", e.target.checked)} className="h-4 w-4 rounded border-border" />
              <span className="inline-flex items-center gap-1"><Ban size={13} className="text-error" /> {t("Blocks promotion")}</span>
            </label>
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input type="checkbox" disabled={!editable} checked={!!meta.affectsReward}
                onChange={(e) => set("affectsReward", e.target.checked)} className="h-4 w-4 rounded border-border" />
              <span className="inline-flex items-center gap-1"><Award size={13} className="text-error" /> {t("Blocks reward")}</span>
            </label>
            {/* Opt-OUT, so it renders checked unless explicitly false — including for records saved
                before the field existed. */}
            <label className="flex cursor-pointer items-center gap-2 text-sm">
              <input type="checkbox" disabled={!editable} checked={meta.affectsSalaryIncrement !== false}
                onChange={(e) => set("affectsSalaryIncrement", e.target.checked)} className="h-4 w-4 rounded border-border" />
              <span className="inline-flex items-center gap-1"><TrendingUp size={13} className="text-error" /> {t("Blocks salary increment")}</span>
            </label>
          </div>
        </div>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {editable && (
        <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
          <button type="button" disabled={isSaving || !canSave} onClick={submit}
            className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">
            <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : id ? t("Save Changes") : t("Raise Disciplinary Case")}
          </button>
        </div>
      )}
    </div>
  );
}

export default memo(DisciplinaryCaseForm);
