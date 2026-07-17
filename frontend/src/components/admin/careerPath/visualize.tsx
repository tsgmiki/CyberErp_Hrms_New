"use client";
import { memo, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Milestone, CheckCircle2, Circle } from "lucide-react";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import { getCareerPathVisualize } from "@/services/admin/careerPath/visualize";
import getAllEmployee from "@/services/admin/employee/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { careerStepProgressStatusOptions } from "@/constants/careerDevelopment";
import Loading from "../../common/loader/loader";

const empName = (e: any) => `${e.fullName ?? `${e.firstName ?? ""} ${e.grandFatherName ?? ""}`.trim()}${e.employeeNumber ? ` (${e.employeeNumber})` : ""}`;
const progressLabel = (v?: string) => careerStepProgressStatusOptions.find((o) => o.id === v)?.name ?? v ?? "";

/** Career-path ladder (HC166) with an optional employee overlay of progress + competency gaps (HC164/HC165). */
function CareerPathLadder({ pathId }: { pathId: string }) {
  const [employeeId, setEmployeeId] = useState<string>("");

  const { data: employees } = useQuery({
    queryKey: ["employees", "ladderPicker"],
    queryFn: () => getAllEmployee({ ...parameterInitialData, take: 500 }),
    staleTime: 60_000,
  });
  const employeeOptions = useMemo(() => (employees?.data ?? []).map((e) => ({ id: e.id!, name: empName(e) })), [employees]);
  const eName = (id?: string) => employeeOptions.find((o) => o.id === id)?.name ?? "";

  const { data, isLoading } = useQuery({
    queryKey: ["careerPathVisualize", pathId, employeeId],
    queryFn: () => getCareerPathVisualize(pathId, employeeId || undefined),
    enabled: !!pathId,
  });

  return (
    <section className="rounded-xl border border-border bg-card p-4 shadow-sm">
      <div className="mb-3 flex flex-wrap items-center gap-3">
        <h3 className="mr-auto flex items-center gap-2 text-sm font-semibold text-foreground"><Milestone size={15} /> Path Ladder</h3>
        <div className="w-72">
          <FormUtility component={{ name: "ladderEmployee", label: "", type: "dropDown", layout: "auth", placeholder: "Overlay an employee…", value: employeeId, displayValue: eName(employeeId), data: employeeOptions as never, onSelect: (_n, r: any) => setEmployeeId(r.id) }} />
        </div>
        {employeeId && (
          <button type="button" onClick={() => setEmployeeId("")} className="rounded border border-border px-2 py-1 text-xs text-muted hover:text-foreground">Clear</button>
        )}
      </div>

      {employeeId && data?.progressPercent != null && (
        <p className="mb-3 text-xs text-muted">{data.employeeName} — overall progress <span className="font-semibold text-foreground">{Number(data.progressPercent).toFixed(0)}%</span></p>
      )}

      {isLoading ? (
        <Loading />
      ) : (data?.steps?.length ?? 0) === 0 ? (
        <p className="text-center text-xs text-muted">No steps yet — add steps above.</p>
      ) : (
        <div className="flex flex-col gap-2">
          {(data?.steps ?? []).map((s) => (
            <div key={s.stepId} className={`rounded-lg border p-3 ${s.isCurrentStep ? "border-primary/50 bg-primary/5" : "border-border bg-secondary/20"}`}>
              <div className="flex items-center gap-2">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">{s.stepOrder}</span>
                <span className="min-w-0 flex-1 truncate text-sm font-medium text-foreground">{s.name}</span>
                {s.positionClassName && <span className="hidden text-xs text-muted sm:block">{s.positionClassName}</span>}
                {employeeId && s.progressStatus && (
                  <span className="inline-flex items-center gap-1 rounded px-2 py-0.5 text-xs font-semibold text-muted">
                    {s.progressStatus === "Completed" ? <CheckCircle2 size={12} className="text-success" /> : <Circle size={12} />} {progressLabel(s.progressStatus)}
                  </span>
                )}
                {s.isCurrentStep && <span className="rounded bg-primary/15 px-2 py-0.5 text-xs font-semibold text-primary">Current</span>}
              </div>
              {s.competencies.length > 0 && (
                <div className="mt-2 flex flex-wrap gap-1.5 pl-8">
                  {s.competencies.map((c) => (
                    <span key={c.competencyId}
                      className={`rounded-full border px-2 py-0.5 text-xs ${employeeId ? (c.isMet ? "border-success/40 bg-success/10 text-success" : "border-warning/40 bg-warning/10 text-warning") : "border-border bg-card text-muted"}`}>
                      {c.name}
                    </span>
                  ))}
                  {employeeId && <span className="text-xs text-muted">({s.metCount}/{s.requiredCount} met)</span>}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </section>
  );
}

export default memo(CareerPathLadder);
