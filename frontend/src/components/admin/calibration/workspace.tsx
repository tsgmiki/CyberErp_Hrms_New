"use client";
import { memo, useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Save, CheckCircle2, Lock } from "lucide-react";
import type { CalibrationItemModel } from "@/models";
import { getCalibrationSession, saveCalibrationItem, finalizeCalibrationSession } from "@/services/admin/calibration";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const SCORE = "w-24 rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";
const TEXT = "w-full rounded-md border border-border bg-card px-2 py-1 text-sm text-foreground focus:border-primary focus:outline-none";

const numOrNull = (v: unknown): number | null => {
  if (v === "" || v === null || typeof v === "undefined") return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
};

function CalibrationWorkspace({ id }: { id: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();

  const { data: session, isLoading } = useQuery({
    queryKey: ["calibrationSession", id],
    queryFn: () => getCalibrationSession(id),
    enabled: id !== "",
  });

  const [items, setItems] = useState<CalibrationItemModel[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isBusy, setIsBusy] = useState(false);

  useEffect(() => {
    if (session) setItems(session.items ?? []);
  }, [session]);

  const finalized = session?.status === "Finalized";

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ["calibrationSession", id] });
    queryClient.invalidateQueries({ queryKey: ["calibrationSessions"] });
  };

  const patch = (itemId: string, field: keyof CalibrationItemModel, value: unknown) =>
    setItems((p) => p.map((i) => (i.id === itemId ? { ...i, [field]: value } : i)));

  const saveItem = async (item: CalibrationItemModel) => {
    setIsBusy(true);
    const result = await saveCalibrationItem({
      itemId: item.id as string,
      calibratedScore: numOrNull(item.calibratedScore),
      justification: item.justification,
    });
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  const finalize = async () => {
    setIsBusy(true);
    const result = await finalizeCalibrationSession(id);
    setFormState(result);
    setIsBusy(false);
    if (result.status === "success") refresh();
  };

  if (isLoading || !session) return <Loading />;

  return (
    <div className="space-y-5 text-foreground">
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="flex flex-wrap items-start justify-between gap-3">
          <div>
            <h2 className="text-base font-semibold">{session.name}</h2>
            <p className="text-xs text-muted">{session.reviewCycleName}{session.organizationUnitName ? ` · ${session.organizationUnitName}` : ""}</p>
          </div>
          <span className={`inline-flex items-center gap-1 rounded-full px-3 py-1 text-xs font-semibold ${finalized ? "bg-secondary/60 text-muted" : "bg-primary/10 text-primary"}`}>
            {finalized && <Lock className="h-3 w-3" />} {session.status}
          </span>
        </div>
        {session.notes && <p className="mt-2 text-xs text-muted">{session.notes}</p>}
      </section>

      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Cohort")}</h3>
        {items.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No appraisals in this cohort.")}</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-[13px]">
              <thead>
                <tr className="border-b border-border text-left text-xs uppercase tracking-wide text-table-header">
                  <th className="px-2 py-2 font-semibold">{t("Employee")}</th>
                  <th className="px-2 py-2 font-semibold">{t("Original")}</th>
                  <th className="px-2 py-2 font-semibold">{t("Calibrated")}</th>
                  <th className="px-2 py-2 font-semibold">{t("Justification")}</th>
                  {!finalized && <th className="px-2 py-2" />}
                </tr>
              </thead>
              <tbody>
                {items.map((i) => (
                  <tr key={i.id} className="border-b border-border/60 align-top">
                    <td className="px-2 py-2 text-foreground">{i.employeeName}</td>
                    <td className="px-2 py-2 text-muted">{i.originalScore ?? "—"}</td>
                    <td className="px-2 py-2">
                      {finalized ? (
                        <span className="font-semibold">{i.calibratedScore ?? "—"}</span>
                      ) : (
                        <input type="number" step="any" className={SCORE} value={i.calibratedScore ?? ""} onChange={(e) => patch(i.id as string, "calibratedScore", e.target.value)} />
                      )}
                    </td>
                    <td className="px-2 py-2">
                      {finalized ? (
                        <span className="text-muted">{i.justification ?? "—"}</span>
                      ) : (
                        <input className={TEXT} value={i.justification ?? ""} onChange={(e) => patch(i.id as string, "justification", e.target.value)} placeholder={t("Reason for adjustment") ?? ""} />
                      )}
                    </td>
                    {!finalized && (
                      <td className="px-2 py-1.5">
                        <button type="button" disabled={isBusy} onClick={() => saveItem(i)} className="inline-flex items-center gap-1 rounded border border-border px-2.5 py-1 text-xs font-semibold hover:bg-secondary/40 disabled:opacity-50">
                          <Save className="h-3.5 w-3.5" /> {t("Save")}
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {!finalized && (
        <div className="flex justify-end">
          <button type="button" disabled={isBusy} onClick={finalize} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
            <CheckCircle2 className="h-4 w-4" /> {t("Finalize Calibration")}
          </button>
        </div>
      )}
    </div>
  );
}

export default memo(CalibrationWorkspace);
