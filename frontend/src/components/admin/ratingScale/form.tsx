"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save } from "lucide-react";
import type { RatingScaleModel, RatingScaleLevelModel } from "@/models";
import { getRatingScale, saveRatingScale } from "@/services/admin/ratingScale";
import { ratingScoreTypeOptions } from "@/constants/performance";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableLevel extends RatingScaleLevelModel {
  _key: number;
}

/** Coerce a text input to a number, or undefined when blank. */
const num = (v: unknown): number | undefined => {
  if (v === "" || v === null || typeof v === "undefined") return undefined;
  const n = Number(v);
  return Number.isFinite(n) ? n : undefined;
};

function RatingScaleForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<RatingScaleModel>({ scoreType: "Numeric", isActive: true, sortOrder: 0 });
  const [levels, setLevels] = useState<EditableLevel[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["ratingScale", id],
    queryFn: () => getRatingScale(id),
    enabled: id !== "",
  });

  useEffect(() => {
    if (record) {
      setMeta(record);
      setLevels((record.levels ?? []).map((l) => ({ ...l, _key: nextKey() })));
    }
  }, [record]);

  const setMetaField = (name: keyof RatingScaleModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  const addLevel = () =>
    setLevels((p) => [
      ...p,
      { _key: nextKey(), value: p.length + 1, label: "", sortOrder: p.length },
    ]);
  const updateLevel = (key: number, patch: Partial<EditableLevel>) =>
    setLevels((p) => p.map((l) => (l._key === key ? { ...l, ...patch } : l)));
  const removeLevel = (key: number) => setLevels((p) => p.filter((l) => l._key !== key));

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSaving(true);
    const payload: RatingScaleModel = {
      ...meta,
      sortOrder: num(meta.sortOrder) ?? 0,
      levels: levels.map(({ _key, ...l }, i) => ({
        ...l,
        value: num(l.value) ?? 0,
        minScore: num(l.minScore) ?? null,
        maxScore: num(l.maxScore) ?? null,
        sortOrder: i,
      })),
    };
    const result = await saveRatingScale(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["ratingScalesList"] });
      queryClient.invalidateQueries({ queryKey: ["ratingScales"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  return (
    <form onSubmit={submit} className="space-y-5 text-foreground">
      {/* Scale metadata */}
      <section className="rounded-lg border border-border bg-card p-4">
        <h3 className="mb-3 text-sm font-semibold">{t("Rating Scale")}</h3>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div>
            <label className={LABEL}>{t("Name")} *</label>
            <input className={INPUT} value={meta.name ?? ""} onChange={(e) => setMetaField("name", e.target.value)} placeholder="e.g. 1-5 Scale" required />
          </div>
          <div>
            <label className={LABEL}>{t("Score Type")}</label>
            <select className={INPUT} value={meta.scoreType ?? "Numeric"} onChange={(e) => setMetaField("scoreType", e.target.value)}>
              {ratingScoreTypeOptions.map((o) => (
                <option key={o.id} value={o.id}>{t(o.name)}</option>
              ))}
            </select>
          </div>
          <div>
            <label className={LABEL}>{t("Sort Order")}</label>
            <input type="number" className={INPUT} value={meta.sortOrder ?? 0} onChange={(e) => setMetaField("sortOrder", e.target.value)} />
          </div>
          <div className="flex items-end gap-2 pb-1">
            <input id="rs-active" type="checkbox" className="h-4 w-4 accent-primary" checked={meta.isActive ?? true} onChange={(e) => setMetaField("isActive", e.target.checked)} />
            <label htmlFor="rs-active" className="text-sm">{t("Active")}</label>
          </div>
          <div className="sm:col-span-2">
            <label className={LABEL}>{t("Description")}</label>
            <input className={INPUT} value={meta.description ?? ""} onChange={(e) => setMetaField("description", e.target.value)} />
          </div>
        </div>
      </section>

      {/* Levels */}
      <section className="rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex items-center justify-between">
          <h3 className="text-sm font-semibold">{t("Levels")}</h3>
          <button type="button" onClick={addLevel} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
            <Plus className="h-3.5 w-3.5" /> {t("Add Level")}
          </button>
        </div>

        {levels.length === 0 ? (
          <p className="py-6 text-center text-sm text-muted">{t("No levels yet. Add at least one level.")}</p>
        ) : (
          <div className="space-y-2">
            {levels.map((l) => (
              <div key={l._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[90px_1fr_110px_110px_auto]">
                <div>
                  <label className={LABEL}>{t("Value")} *</label>
                  <input type="number" className={INPUT} value={l.value ?? ""} onChange={(e) => updateLevel(l._key, { value: e.target.value as never })} required />
                </div>
                <div>
                  <label className={LABEL}>{t("Label")} *</label>
                  <input className={INPUT} value={l.label ?? ""} onChange={(e) => updateLevel(l._key, { label: e.target.value })} required />
                </div>
                <div>
                  <label className={LABEL}>{t("Min Score")}</label>
                  <input type="number" step="any" className={INPUT} value={l.minScore ?? ""} onChange={(e) => updateLevel(l._key, { minScore: e.target.value as never })} />
                </div>
                <div>
                  <label className={LABEL}>{t("Max Score")}</label>
                  <input type="number" step="any" className={INPUT} value={l.maxScore ?? ""} onChange={(e) => updateLevel(l._key, { maxScore: e.target.value as never })} />
                </div>
                <div className="flex items-center pb-1">
                  <button type="button" onClick={() => removeLevel(l._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </section>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      <div className="flex justify-end">
        <button type="submit" disabled={isSaving} className="inline-flex items-center gap-2 rounded-md bg-primary px-4 py-2 text-sm font-semibold text-on-accent hover:opacity-90 disabled:opacity-50">
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Rating Scale")}
        </button>
      </div>
    </form>
  );
}

export default memo(RatingScaleForm);
