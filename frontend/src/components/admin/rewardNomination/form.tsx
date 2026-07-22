"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, Award, ShieldAlert } from "lucide-react";
import type { RewardNominationModel } from "@/models";
import { getNomination, saveNomination } from "@/services/admin/rewardNomination";
import { getDisciplinaryEligibility } from "@/services/admin/disciplinaryCase";
import getAllRecognitionBadge from "@/services/admin/recognitionBadge/getAll";
import getAllRecognitionProgram from "@/services/admin/recognitionProgram/getAll";
import EmployeePicker from "@/components/common/employeePicker";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";
const LABEL = "block text-xs font-medium text-muted mb-1";

function RewardNominationForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [meta, setMeta] = useState<RewardNominationModel>({});
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["rewardNomination", id],
    queryFn: () => getNomination(id),
    enabled: id !== "",
  });

  const [badgeParam] = useState({ ...parameterInitialData, take: 200, status: "true" });
  const { data: badges } = useQuery({ queryKey: ["recognitionBadges", badgeParam], queryFn: () => getAllRecognitionBadge(badgeParam) });
  const [progParam] = useState({ ...parameterInitialData, take: 100, status: "true" });
  const { data: programs } = useQuery({ queryKey: ["recognitionPrograms", progParam], queryFn: () => getAllRecognitionProgram(progParam) });

  useEffect(() => {
    if (record) setMeta(record);
  }, [record]);

  const set = (name: keyof RewardNominationModel, value: unknown) => setMeta((p) => ({ ...p, [name]: value }));
  const editable = !id || meta.status === "Pending";

  // HC224/HC225 — surface a disciplinary block on the nominee (the server also hard-blocks the save).
  const { data: eligibility } = useQuery({
    queryKey: ["disciplineEligibility", meta.nomineeEmployeeId],
    queryFn: () => getDisciplinaryEligibility(meta.nomineeEmployeeId!),
    enabled: !!meta.nomineeEmployeeId && editable,
    staleTime: 30_000,
  });
  const rewardBlocked = !!eligibility?.isBlockedForReward;

  const selectedBadge = badges?.data?.find((b) => b.id === meta.recognitionBadgeId);
  const selectedProgram = programs?.data?.find((p) => p.id === meta.recognitionProgramId);

  const submit = async () => {
    setIsSaving(true);
    const result = await saveNomination({
      id: meta.id,
      nomineeEmployeeId: meta.nomineeEmployeeId,
      recognitionBadgeId: meta.recognitionBadgeId,
      recognitionProgramId: meta.recognitionProgramId || undefined,
      reason: meta.reason,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["rewardNominations"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = editable && !!meta.nomineeEmployeeId && !!meta.recognitionBadgeId && !!meta.reason && !rewardBlocked;

  return (
    <div className="space-y-4 text-foreground">
      {id && meta.status && meta.status !== "Pending" && (
        <p className="rounded-lg border border-border bg-secondary/20 px-3 py-2 text-xs text-muted">
          {t("This nomination is")} <span className="font-semibold text-foreground">{meta.status}</span> — {t("it can no longer be edited here.")}
        </p>
      )}

      <div className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 flex items-center gap-2 text-sm font-semibold">
          <Award size={16} className="text-primary" /> {t("Nomination")}
        </h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Nominee")} *</label>
            {/* Role-scoped: HR = all, manager = unit subtree; self-nomination is refused server-side. */}
            <EmployeePicker
              value={meta.nomineeEmployeeId}
              displayValue={meta.nomineeName}
              disabled={!editable}
              onSelect={(eid, name) => setMeta((p) => ({ ...p, nomineeEmployeeId: eid, nomineeName: name }))}
            />
            {rewardBlocked && (
              <p className="mt-1.5 flex items-start gap-1.5 rounded-md border border-error/30 bg-error/10 px-2.5 py-1.5 text-xs text-error">
                <ShieldAlert size={13} className="mt-0.5 shrink-0" />
                {t("This employee has an active disciplinary measure that blocks reward — the nomination cannot be submitted.")}
              </p>
            )}
          </div>
          <div>
            <label className={LABEL}>{t("Award")} *</label>
            <select className={INPUT} disabled={!editable} value={meta.recognitionBadgeId ?? ""} onChange={(e) => set("recognitionBadgeId", e.target.value)}>
              <option value="">{t("Select an award")}</option>
              {(badges?.data ?? []).map((b) => (
                <option key={b.id} value={b.id}>{b.name}</option>
              ))}
              {meta.recognitionBadgeId && !selectedBadge && <option value={meta.recognitionBadgeId}>{meta.badgeName}</option>}
            </select>
            {selectedBadge && (
              <p className="mt-1 text-xs text-muted">
                {selectedBadge.rewardKind}
                {selectedBadge.pointsValue ? ` · ${selectedBadge.pointsValue} ${t("pts")}` : ""}
                {selectedBadge.monetaryValue ? ` · ${selectedBadge.monetaryValue}` : ""}
                {selectedBadge.criteria ? ` — ${selectedBadge.criteria}` : ""}
              </p>
            )}
          </div>
          <div>
            <label className={LABEL}>{t("Program")}</label>
            <select className={INPUT} disabled={!editable} value={meta.recognitionProgramId ?? ""} onChange={(e) => set("recognitionProgramId", e.target.value || undefined)}>
              <option value="">{t("None (ad-hoc nomination)")}</option>
              {(programs?.data ?? []).map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
              {meta.recognitionProgramId && !selectedProgram && <option value={meta.recognitionProgramId}>{meta.programName}</option>}
            </select>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Reason")} *</label>
            <textarea
              className={INPUT}
              rows={3}
              disabled={!editable}
              placeholder={t("Why does this employee deserve the award?")}
              value={meta.reason ?? ""}
              onChange={(e) => set("reason", e.target.value)}
            />
          </div>
        </div>
        {editable && (
          <div className="mt-4 flex justify-end">
            <button
              type="button"
              disabled={!canSave || isSaving}
              onClick={submit}
              className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3.5 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50"
            >
              <Save size={14} /> {isSaving ? t("Saving…") : t("Submit Nomination")}
            </button>
          </div>
        )}
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default memo(RewardNominationForm);
