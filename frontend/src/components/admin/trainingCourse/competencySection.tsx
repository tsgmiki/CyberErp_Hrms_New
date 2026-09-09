"use client";
import { memo, useEffect, useMemo, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Target, Save, Star } from "lucide-react";
import getAllCompetency from "@/services/admin/competency/getAll";
import { getCourseCompetencies, setCourseCompetencies } from "@/services/admin/courseCompetency";
import { parameterInitialData } from "@/constants/initialization";
import { toast } from "@/components/common/toast";
import Loading from "@/components/common/loader/loader";

/**
 * Which competencies this course develops.
 *
 * <p>The mapping is what turns the catalogue into a targeting engine: a competency gap on an
 * appraisal can then name a real course instead of only reporting a low score. Nothing else in the
 * product records what completing a course teaches.</p>
 *
 * <p>Saved through its own endpoint rather than with the course form, because it needs a course id
 * to attach to — so it appears only once the course exists, the same rule the employee-profile child
 * collections follow.</p>
 */
function CourseCompetencySection({ trainingCourseId }: { trainingCourseId?: string }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [selected, setSelected] = useState<Record<string, boolean>>({});
  const [busy, setBusy] = useState(false);
  const [dirty, setDirty] = useState(false);

  const enabled = !!trainingCourseId;

  const { data: mapped, isLoading: mappedLoading } = useQuery({
    queryKey: ["courseCompetencies", trainingCourseId],
    queryFn: () => getCourseCompetencies(trainingCourseId as string),
    enabled,
  });

  const { data: all, isLoading: allLoading } = useQuery({
    queryKey: ["competencies", "course-mapping"],
    queryFn: () => getAllCompetency({ ...parameterInitialData, take: 300 } as never),
    enabled,
  });

  // Re-seed from the server whenever the saved mapping changes, so a save leaves the editor showing
  // what was stored rather than the keystrokes that produced it.
  useEffect(() => {
    const next: Record<string, boolean> = {};
    for (const m of mapped ?? []) next[m.competencyId] = m.isPrimary;
    setSelected(next);
    setDirty(false);
  }, [mapped]);

  const competencies = useMemo(
    () => (all?.data ?? []).filter((c) => c.isActive !== false),
    [all],
  );

  // Grouped by category because a long flat list of competencies is unusable to scan; the category
  // is how people already think about them elsewhere in Performance.
  const grouped = useMemo(() => {
    const by = new Map<string, typeof competencies>();
    for (const c of competencies) {
      const key = c.competencyCategoryName || t("Uncategorised");
      if (!by.has(key)) by.set(key, []);
      by.get(key)!.push(c);
    }
    return [...by.entries()].sort((a, b) => a[0].localeCompare(b[0]));
  }, [competencies, t]);

  if (!enabled) {
    return (
      <div className="rounded-lg border border-dashed border-border bg-card/40 p-4 text-sm text-muted">
        {t("Save the course first — competencies attach to a saved course.")}
      </div>
    );
  }
  if (mappedLoading || allLoading) return <Loading />;

  const toggle = (id: string) => {
    setSelected((p) => {
      const next = { ...p };
      if (id in next) delete next[id];
      else next[id] = true; // a newly mapped competency starts primary — the common case
      return next;
    });
    setDirty(true);
  };

  const togglePrimary = (id: string) => {
    setSelected((p) => (id in p ? { ...p, [id]: !p[id] } : p));
    setDirty(true);
  };

  const save = async () => {
    setBusy(true);
    const res = await setCourseCompetencies(
      trainingCourseId as string,
      Object.entries(selected).map(([competencyId, isPrimary]) => ({ competencyId, isPrimary })),
    );
    setBusy(false);
    if (res.status === "success") {
      toast.success(res.message);
      queryClient.invalidateQueries({ queryKey: ["courseCompetencies", trainingCourseId] });
      // Suggestions read this mapping, so they are stale the moment it changes.
      queryClient.invalidateQueries({ queryKey: ["trainingNeedSuggestions"] });
      setDirty(false);
    } else {
      toast.error(res.message);
    }
  };

  const count = Object.keys(selected).length;

  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <div className="mb-3 flex items-start justify-between gap-3">
        <div className="flex items-start gap-2">
          <span className="mt-0.5 flex h-8 w-8 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
            <Target className="h-4 w-4" />
          </span>
          <div>
            <h3 className="text-sm font-semibold text-foreground">{t("Competencies Developed")}</h3>
            <p className="text-xs text-muted">
              {t("Tick what this course builds. A gap in one of these will recommend this course.")}
            </p>
          </div>
        </div>
        <button
          type="button"
          disabled={busy || !dirty}
          onClick={save}
          className="inline-flex shrink-0 items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90 disabled:opacity-40"
        >
          <Save className="h-3.5 w-3.5" />
          {busy ? t("Saving…") : dirty ? t("Save Competencies") : t("Saved")}
        </button>
      </div>

      <p className="mb-3 text-xs text-muted">
        {count === 0
          ? t("None mapped — this course will never be recommended for a competency gap.")
          : `${count} ${t("mapped")} · ${Object.values(selected).filter(Boolean).length} ${t("primary")}`}
      </p>

      <div className="max-h-80 space-y-3 overflow-auto pr-1">
        {grouped.map(([category, items]) => (
          <div key={category}>
            <p className="mb-1 text-[11px] font-semibold uppercase tracking-wide text-muted">{category}</p>
            <div className="space-y-1">
              {items.map((c) => {
                const id = c.id as string;
                const on = id in selected;
                return (
                  <div
                    key={id}
                    className={`flex items-center gap-2 rounded-md border px-2.5 py-1.5 transition ${
                      on ? "border-primary/40 bg-primary/5" : "border-border bg-secondary/20"
                    }`}
                  >
                    <input
                      type="checkbox"
                      checked={on}
                      onChange={() => toggle(id)}
                      className="h-4 w-4 shrink-0 accent-primary"
                      aria-label={c.name}
                    />
                    <span className="min-w-0 flex-1 truncate text-sm text-foreground">{c.name}</span>
                    {on && (
                      <button
                        type="button"
                        onClick={() => togglePrimary(id)}
                        // Primary vs supporting decides recommendation order, so it is only
                        // meaningful — and only shown — once the competency is actually mapped.
                        title={
                          selected[id]
                            ? (t("Primary — recommended first for this competency") ?? undefined)
                            : (t("Supporting — recommended after primary courses") ?? undefined)
                        }
                        className={`inline-flex shrink-0 items-center gap-1 rounded px-1.5 py-0.5 text-[11px] font-semibold ${
                          selected[id] ? "bg-primary/15 text-primary" : "bg-secondary text-muted"
                        }`}
                      >
                        <Star className="h-3 w-3" fill={selected[id] ? "currentColor" : "none"} />
                        {selected[id] ? t("Primary") : t("Supporting")}
                      </button>
                    )}
                  </div>
                );
              })}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default memo(CourseCompetencySection);
