"use client";
import { memo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Scale } from "lucide-react";
import { createCalibrationSession } from "@/services/admin/calibration";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import { StatusMessage } from "../../common/statusMessage/status";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

/** Create a calibration session — the cohort's appraisals are auto-pulled server-side. */
function CalibrationCreate({ onCreated }: { onCreated: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [name, setName] = useState("");
  const [reviewCycleId, setReviewCycleId] = useState("");
  const [organizationUnitId, setOrganizationUnitId] = useState("");
  const [notes, setNotes] = useState("");
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const [cycleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: cycles } = useQuery({ queryKey: ["reviewCycles", cycleParam], queryFn: () => getAllReviewCycle(cycleParam) });
  const [unitParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: units } = useQuery({ queryKey: ["organizationUnits", unitParam], queryFn: () => getAllOrganizationUnit(unitParam) });

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const result = await createCalibrationSession({
      name,
      reviewCycleId,
      organizationUnitId: organizationUnitId || undefined,
      notes: notes || undefined,
    });
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success" && result.id) {
      queryClient.invalidateQueries({ queryKey: ["calibrationSessions"] });
      onCreated(result.id);
    }
  };

  return (
    <form onSubmit={submit} className="space-y-5 p-1 text-foreground">
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("New Calibration Session")}</h3>
        <p className="mb-3 text-xs text-muted">{t("Assembles the cycle's appraisals (optionally a single unit) for score normalization.")}</p>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Name")} *</label>
            <input className={INPUT} value={name} onChange={(e) => setName(e.target.value)} placeholder="e.g. FY26 Moderation" required />
          </div>
          <div>
            <label className={LABEL}>{t("Review Cycle")} *</label>
            <select className={INPUT} value={reviewCycleId} onChange={(e) => setReviewCycleId(e.target.value)} required>
              <option value="">{t("Select cycle")}</option>
              {(cycles?.data ?? []).map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Organization Unit")}</label>
            <select className={INPUT} value={organizationUnitId} onChange={(e) => setOrganizationUnitId(e.target.value)}>
              <option value="">{t("All units")}</option>
              {(units?.data ?? []).map((u) => (
                <option key={u.id} value={u.id}>{u.name}</option>
              ))}
            </select>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Notes")}</label>
            <input className={INPUT} value={notes} onChange={(e) => setNotes(e.target.value)} />
          </div>
        </div>
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button type="submit" disabled={isSaving} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Scale className="h-4 w-4" /> {isSaving ? t("Creating…") : t("Create Session")}
        </button>
      </div>
    </form>
  );
}

export default memo(CalibrationCreate);
