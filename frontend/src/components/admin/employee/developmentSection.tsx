"use client";
import { memo } from "react";
import { useQuery } from "@tanstack/react-query";
import { Gauge, Route, GitBranchPlus, Handshake, Target } from "lucide-react";
import { FormSection, EmptyHint } from "@/components/common/formSection";
import Loading from "../../common/loader/loader";
import { getEmployeeDevelopmentProfile } from "@/services/admin/employee/developmentProfile";
import { readinessLevelOptions, mentorshipContextOptions } from "@/constants/careerDevelopment";

const READY_TONE: Record<string, string> = {
  ReadyNow: "bg-success/15 text-success",
  Ready1To2Years: "bg-info/15 text-info",
  Ready3PlusYears: "bg-warning/15 text-warning",
  NotReady: "bg-muted/30 text-muted",
};
const STATUS_TONE: Record<string, string> = {
  Active: "bg-info/15 text-info",
  Completed: "bg-success/15 text-success",
  OnHold: "bg-warning/15 text-warning",
};
const readyLabel = (v: string) => readinessLevelOptions.find((o) => o.id === v)?.name ?? v;
const ctxLabel = (v: string) => mentorshipContextOptions.find((o) => o.id === v)?.name ?? v;

function Stat({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col rounded-lg border border-border/60 bg-background/50 p-3">
      <span className="text-[10px] uppercase tracking-wide text-muted">{label}</span>
      <span className="text-lg font-semibold text-foreground">{value}</span>
    </div>
  );
}

/** Employee 360 "Development" tab (HC158) — the bridge between Performance and Career Development. */
function DevelopmentSection({ employeeId }: { employeeId: string }) {
  const { data, isLoading } = useQuery({
    queryKey: ["employeeDevelopmentProfile", employeeId],
    queryFn: () => getEmployeeDevelopmentProfile(employeeId),
    enabled: !!employeeId,
  });

  if (isLoading) return <Loading />;
  if (!data) return <EmptyHint>No development data.</EmptyHint>;

  const perf = data.performance;
  const gap = data.nextStepGap;

  return (
    <div className="space-y-4 p-1">
      {/* Performance snapshot (HC153/HC158) */}
      <FormSection title="Performance" description="Latest appraisal and goal activity." icon={Gauge}>
        <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
          <Stat label="Latest appraisal" value={perf?.latestAppraisal?.overallScore != null ? String(perf.latestAppraisal.overallScore) : "—"} />
          <Stat label="Active goals" value={String(perf?.activeGoals ?? 0)} />
          <Stat label="Achievements" value={String(perf?.achievementsCount ?? 0)} />
          <Stat label="Recognitions" value={String(perf?.recognitionsCount ?? 0)} />
        </div>
        {perf?.latestAppraisal?.finalRatingLabel ? (
          <p className="mt-2 text-xs text-muted">Rating: <span className="font-medium text-foreground">{perf.latestAppraisal.finalRatingLabel}</span>{perf.latestAppraisal.acknowledgmentStatus ? ` · ${perf.latestAppraisal.acknowledgmentStatus}` : ""}</p>
        ) : null}
      </FormSection>

      {/* Career paths (HC163/HC165) */}
      <FormSection title="Career Paths" description="Assigned progression tracks and progress." icon={Route}>
        {data.careerPaths.length === 0 ? (
          <EmptyHint>Not assigned to a career path yet.</EmptyHint>
        ) : (
          <div className="space-y-2">
            {data.careerPaths.map((c) => (
              <div key={c.id} className="flex items-center gap-3 rounded-lg border border-border/60 bg-background/50 px-3 py-2">
                <span className="min-w-0 flex-1 truncate text-sm font-medium text-foreground">{c.careerPathName ?? "—"}</span>
                <span className={`rounded px-2 py-0.5 text-xs font-semibold ${STATUS_TONE[c.status] ?? "bg-muted/30 text-muted"}`}>{c.status}</span>
                <div className="h-1.5 w-24 overflow-hidden rounded-full bg-secondary"><div className="h-full rounded-full bg-primary" style={{ width: `${Number(c.progressPercent)}%` }} /></div>
                <span className="w-9 text-right text-xs font-semibold tabular-nums text-muted">{Number(c.progressPercent).toFixed(0)}%</span>
              </div>
            ))}
          </div>
        )}
      </FormSection>

      {/* Next-step competency gap (HC164) */}
      {gap && gap.targetStepId ? (
        <FormSection title="Development Gap" description={`Competencies to develop for the next step: ${gap.targetStepName ?? "—"}.`} icon={Target}>
          {gap.recommendations.length === 0 ? (
            <p className="text-xs text-success">No gaps — the employee meets the next step's requirements.</p>
          ) : (
            <div className="flex flex-wrap gap-1.5">
              {gap.recommendations.map((r) => (
                <span key={r.competencyId} className="rounded-full border border-warning/40 bg-warning/10 px-2 py-0.5 text-xs text-warning">{r.name} · {r.suggestedAction}</span>
              ))}
            </div>
          )}
        </FormSection>
      ) : null}

      {/* Succession candidacies (HC153/HC154) */}
      <FormSection title="Succession Candidacy" description="Roles this employee is a potential successor for." icon={GitBranchPlus}>
        {data.successionCandidacies.length === 0 ? (
          <EmptyHint>Not listed as a successor for any role.</EmptyHint>
        ) : (
          <div className="space-y-2">
            {data.successionCandidacies.map((s) => (
              <div key={s.id} className="flex items-center gap-3 rounded-lg border border-border/60 bg-background/50 px-3 py-2">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">#{s.rank}</span>
                <span className="min-w-0 flex-1 truncate text-sm text-foreground"><span className="font-medium">{s.roleTitle ?? "—"}</span> <span className="text-xs text-muted">· {s.planName}</span></span>
                <span className={`rounded px-2 py-0.5 text-xs font-semibold ${READY_TONE[s.readiness] ?? "bg-muted/30 text-muted"}`}>{readyLabel(s.readiness)}</span>
                {s.readinessScore != null ? <span className="text-xs tabular-nums text-muted">{Number(s.readinessScore).toFixed(0)}%</span> : null}
              </div>
            ))}
          </div>
        )}
      </FormSection>

      {/* Mentorships (HC168) */}
      <FormSection title="Mentorships" description="Mentoring relationships." icon={Handshake}>
        {data.mentorships.length === 0 ? (
          <EmptyHint>No mentorships.</EmptyHint>
        ) : (
          <div className="space-y-2">
            {data.mentorships.map((m) => (
              <div key={m.id} className="flex items-center gap-3 rounded-lg border border-border/60 bg-background/50 px-3 py-2 text-sm">
                <span className="rounded bg-secondary px-2 py-0.5 text-xs font-medium text-muted">{m.role}</span>
                <span className="min-w-0 flex-1 truncate text-foreground">{m.counterpartName ?? "—"}</span>
                <span className="text-xs text-muted">{ctxLabel(m.context)} · {m.status}</span>
              </div>
            ))}
          </div>
        )}
      </FormSection>
    </div>
  );
}

export default memo(DevelopmentSection);
