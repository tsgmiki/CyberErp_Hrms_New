"use client";
import { memo } from "react";
import { useTranslation } from "react-i18next";
import { useQuery } from "@tanstack/react-query";
import { TrendingUp, TrendingDown, Minus, Info } from "lucide-react";
import { getEffectiveness } from "@/services/admin/learningCompliance";
import Loading from "@/components/common/loader/loader";

/**
 * A figure with the sample it rests on.
 *
 * <p>A missing number renders as an em dash, never as zero: "0% of 0 results" and "0% of 40 results"
 * mean opposite things, and a dashboard that shows both as 0 is worse than one that shows neither.</p>
 */
function Figure({ value, suffix, sample, sampleLabel }: {
  value?: number | null;
  suffix?: string;
  sample: number;
  sampleLabel: string;
}) {
  const { t } = useTranslation();
  return (
    <div>
      <span className="block tabular-nums text-foreground">
        {value == null ? "—" : `${value}${suffix ?? ""}`}
      </span>
      <span className="block text-[11px] text-muted">
        {sample} {t(sampleLabel)}
      </span>
    </div>
  );
}

/**
 * Did the training work?
 *
 * <p>Levels 1 and 2 of Kirkpatrick are measured here: what participants thought, and what they
 * scored. Level 3 — whether behaviour changed — is a SIGNAL rather than a measurement: it compares
 * appraisal scores on the competencies a course develops, before and after completion. An appraisal
 * is a judgement made for other reasons, so it is evidence worth looking at, not proof.</p>
 *
 * <p>This is the payoff of the earlier phases: the score column only exists because phase 4 measures
 * it, and the competency column only exists because phase 1 records what a course teaches.</p>
 */
function EffectivenessPanel() {
  const { t } = useTranslation();

  const { data, isLoading } = useQuery({
    queryKey: ["trainingEffectiveness"],
    queryFn: () => getEffectiveness(),
  });

  if (isLoading) return <Loading />;

  const rows = (data ?? []).filter((r) => r.completions > 0);

  if (rows.length === 0) {
    return (
      <p className="rounded-lg border border-dashed border-border bg-card/40 p-8 text-center text-sm text-muted">
        {t("No completed training yet — there is nothing to measure.")}
      </p>
    );
  }

  return (
    <div className="space-y-3">
      <p className="flex items-start gap-2 rounded-md border border-primary/25 bg-primary/5 px-3 py-2 text-xs text-foreground">
        <Info className="mt-0.5 h-3.5 w-3.5 shrink-0 text-primary" />
        {t("Reaction and learning are measured. Competency movement is a signal, not proof — an appraisal is a judgement made for other reasons.")}
      </p>

      <div className="overflow-x-auto rounded-lg border border-border bg-card">
        <table className="w-full min-w-[52rem] text-sm">
          <thead>
            <tr className="border-b border-border text-left text-[11px] uppercase tracking-wide text-muted">
              <th className="px-3 py-2 font-semibold">{t("Course")}</th>
              <th className="px-3 py-2 font-semibold">{t("Completions")}</th>
              <th className="px-3 py-2 font-semibold">{t("Reaction")} <span className="normal-case">(1–5)</span></th>
              <th className="px-3 py-2 font-semibold">{t("Learning")}</th>
              <th className="px-3 py-2 font-semibold">{t("Pass rate")}</th>
              <th className="px-3 py-2 font-semibold">{t("Competency movement")}</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => {
              const move = r.competencyMovement;
              const MoveIcon = move == null ? Minus : move > 0 ? TrendingUp : move < 0 ? TrendingDown : Minus;
              const moveTone = move == null || move === 0
                ? "text-muted" : move > 0 ? "text-success" : "text-error";
              return (
                <tr key={r.trainingCourseId} className="border-b border-border align-top last:border-0">
                  <td className="px-3 py-2">
                    <span className="block text-foreground">{r.courseName}</span>
                    {/* Says plainly why a column is empty, instead of leaving the reader to guess. */}
                    {r.caveat && <span className="block text-[11px] text-muted">{t(r.caveat)}</span>}
                  </td>
                  <td className="px-3 py-2 tabular-nums text-foreground">{r.completions}</td>
                  <td className="px-3 py-2">
                    <Figure value={r.averageFeedback} sample={r.feedbackResponses} sampleLabel="response(s)" />
                  </td>
                  <td className="px-3 py-2">
                    <Figure value={r.averageAssessmentScore} suffix="%" sample={r.assessmentResults} sampleLabel="result(s)" />
                  </td>
                  <td className="px-3 py-2">
                    <Figure value={r.firstAttemptPassRate} suffix="%" sample={r.assessmentResults} sampleLabel="assessed" />
                  </td>
                  <td className="px-3 py-2">
                    <span className={`flex items-center gap-1 tabular-nums ${moveTone}`}>
                      <MoveIcon className="h-3.5 w-3.5" />
                      {move == null ? "—" : `${move > 0 ? "+" : ""}${move}`}
                    </span>
                    <span className="block text-[11px] text-muted">
                      {r.competencyPairs} {t("before/after pair(s)")}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default memo(EffectivenessPanel);
