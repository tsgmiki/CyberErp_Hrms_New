"use client";
import { memo, useCallback, useMemo, useState } from "react";
import { ClipboardList, Target } from "lucide-react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Modal from "@/components/common/modal";
import { FormUtility } from "@/components/common/formProvider/formUtility";
import ChildManager, { type ChildColumn } from "@/components/admin/employee/childManager";
import { StatusMessage } from "@/components/common/statusMessage/status";
import { RepeatHeader, RepeatRow, RowRemoveButton, SectionAddButton, EmptyHint } from "@/components/common/formSection";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { FORM_INPUT_CLASS } from "@/components/ui/fieldStyles";
import type { CareerPathStepModel, CareerPathStepCompetencyModel } from "@/models";
import getAllCareerPathStep from "@/services/admin/careerPathStep/getAll";
import getCareerPathStep from "@/services/admin/careerPathStep/get";
import { saveCareerPathStep } from "@/services/admin/careerPathStep/save";
import deleteCareerPathStep from "@/services/admin/careerPathStep/delete";
import getAllPositionClass from "@/services/admin/positionClass/getAll";
import getAllCompetency from "@/services/admin/competency/getAll";
import { parameterInitialData } from "@/constants/initialization";

function CareerPathSteps({ pathId }: { pathId: string }) {
  const queryClient = useQueryClient();
  const [open, setOpen] = useState(false);
  const [form, setForm] = useState<CareerPathStepModel>({});
  const [comps, setComps] = useState<CareerPathStepCompetencyModel[]>([]);
  const [saving, setSaving] = useState(false);
  const [state, setState] = useState<any>({});

  const { data: list, isLoading } = useQuery({
    queryKey: ["careerPathSteps", pathId],
    queryFn: () => getAllCareerPathStep({ ...parameterInitialData, take: 200, parentId: pathId }),
    enabled: !!pathId,
  });
  const { data: positionClasses } = useQuery({
    queryKey: ["positionClasses", "stepLookup"],
    queryFn: () => getAllPositionClass({ ...parameterInitialData, take: 300 }),
    staleTime: 60_000, enabled: open,
  });
  const { data: competencies } = useQuery({
    queryKey: ["competencies", "stepLookup"],
    queryFn: () => getAllCompetency({ ...parameterInitialData, take: 500 }),
    staleTime: 60_000, enabled: open,
  });
  const positionClassOptions = useMemo(
    () => (positionClasses?.data ?? []).map((p) => ({ id: p.id!, name: `${p.code} — ${p.title ?? ""}` })),
    [positionClasses],
  );
  const competencyOptions = useMemo(
    () => (competencies?.data ?? []).map((c) => ({ id: c.id!, name: c.name ?? "" })),
    [competencies],
  );
  const pcName = (id?: string) => positionClassOptions.find((o) => o.id === id)?.name ?? "";
  const compName = (id?: string) => competencyOptions.find((o) => o.id === id)?.name ?? "";

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["careerPathSteps", pathId] });
    queryClient.invalidateQueries({ queryKey: ["careerPathVisualize", pathId] });
  };
  const { mutate: remove } = useMutation({ mutationFn: (id: string) => deleteCareerPathStep(id), onSuccess: invalidate });

  const openAdd = () => { setForm({ stepOrder: (list?.data?.length ?? 0) + 1 }); setComps([]); setState({}); setOpen(true); };
  const openEdit = async (row: CareerPathStepModel) => {
    const full = await getCareerPathStep(row.id!);
    setForm(full ?? row); setComps(full?.competencies ?? []); setState({}); setOpen(true);
  };
  const set = useCallback((name: string, value: unknown) => setForm((p) => ({ ...p, [name]: value })), []);

  const submit = async () => {
    setSaving(true);
    const payload: CareerPathStepModel = {
      ...form, careerPathId: pathId, stepOrder: Number(form.stepOrder ?? 0), name: form.name,
      positionClassId: form.positionClassId || undefined,
      requiredExperienceMonths: form.requiredExperienceMonths != null && String(form.requiredExperienceMonths) !== "" ? Number(form.requiredExperienceMonths) : undefined,
      competencies: comps.filter((c) => c.competencyId).map((c) => ({ competencyId: c.competencyId, weight: c.weight != null && String(c.weight) !== "" ? Number(c.weight) : 1 })),
    };
    const result = await saveCareerPathStep(payload);
    setState(result); setSaving(false);
    if (result.status === "success") { invalidate(); setOpen(false); }
  };

  const columns: ChildColumn<CareerPathStepModel>[] = [
    { name: "stepOrder", label: "Step", render: (v) => `#${v}` },
    { name: "name", label: "Stage" },
    { name: "positionClassName", label: "Target Role", render: (v) => (v as string) || "—" },
    { name: "requiredExperienceMonths", label: "Exp (mo)", render: (v) => (v != null ? String(v) : "—") },
    { name: "competencies", label: "Competencies", render: (v) => String((v as CareerPathStepCompetencyModel[] | undefined)?.length ?? 0) },
  ];

  return (
    <>
      <ChildManager<CareerPathStepModel>
        title="Steps" addLabel="Add Step" columns={columns}
        rows={list?.data} isLoading={isLoading} onAdd={openAdd} onEdit={openEdit} onDelete={(id) => remove(id)}
      />
      {open && (
        <Modal visible size="lg" title={form.id ? "Edit Step" : "Add Step"}
          description="Define a stage of the career path and the competencies it requires."
          onClose={() => setOpen(false)}
          footer={
            <>
              <button type="button" onClick={() => setOpen(false)} className="rounded-lg border border-border px-4 py-2 text-sm font-medium text-foreground transition-colors hover:bg-secondary">Cancel</button>
              <button type="button" disabled={saving || !form.name} onClick={submit} className="rounded-lg bg-primary px-4 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">{saving ? "Saving…" : "Save Step"}</button>
            </>
          }>
          <div className="space-y-4">
            <EntityFormTabs
              hasId
              tabs={[
                {
                  key: "details",
                  label: "Step Details",
                  Icon: ClipboardList,
                  keepMounted: true,
                  content: (
                    <div className="grid min-h-[15rem] grid-cols-1 gap-4 sm:grid-cols-2">
                      <FormUtility component={{ name: "stepOrder", label: "Step Order", type: "text", inputType: "number", layout: "auth", value: form.stepOrder, onChange: (e) => set("stepOrder", e.target.value) }} />
                      <FormUtility component={{ name: "name", label: "Stage Name", required: true, type: "text", layout: "auth", value: form.name, onChange: (e) => set("name", e.target.value) }} />
                      <FormUtility component={{ name: "positionClassId", label: "Target Role (Position Class)", type: "dropDown", layout: "auth", value: form.positionClassId, displayValue: form.positionClassName ?? pcName(form.positionClassId), data: positionClassOptions as never, onSelect: (_n, r: any) => { set("positionClassId", r.id); set("positionClassName", r.name); } }} />
                      <FormUtility component={{ name: "requiredExperienceMonths", label: "Required Experience (months)", type: "text", inputType: "number", layout: "auth", value: form.requiredExperienceMonths, onChange: (e) => set("requiredExperienceMonths", e.target.value) }} />
                      <div className="sm:col-span-2">
                        <FormUtility component={{ name: "certifications", label: "Certifications", type: "text", layout: "auth", value: form.certifications, onChange: (e) => set("certifications", e.target.value) }} />
                      </div>
                      <div className="sm:col-span-2">
                        <FormUtility component={{ name: "description", label: "Description", type: "textarea", layout: "auth", value: form.description, onChange: (e) => set("description", e.target.value) }} />
                      </div>
                    </div>
                  ),
                },
                {
                  key: "competencies",
                  label: "Competencies",
                  Icon: Target,
                  keepMounted: true,
                  content: (
                    <div className="min-h-[15rem]">
                      <div className="mb-3 flex items-center justify-between gap-2">
                        <p className="text-xs text-muted">Competencies required to progress into this step (HC162).</p>
                        <SectionAddButton onClick={() => setComps((c) => [...c, { competencyId: "", weight: 1 }])} label="Add competency" />
                      </div>
                      {comps.length === 0 ? (
                        <EmptyHint>No required competencies yet.</EmptyHint>
                      ) : (
                        <div className="space-y-2">
                          <RepeatHeader cols={["Competency", "Weight", ""]} className="grid-cols-[1fr_120px_36px]" />
                          {comps.map((c, i) => (
                            <RepeatRow key={i} className="grid-cols-[1fr_120px_36px]">
                              <FormUtility component={{ name: `comp-${i}`, label: "", placeholder: "Select competency…", type: "dropDown", value: c.competencyId, displayValue: c.competencyName ?? compName(c.competencyId), data: competencyOptions as never, onSelect: (_n, x: any) => setComps((arr) => arr.map((y, j) => (j === i ? { ...y, competencyId: x.id, competencyName: x.name } : y))) }} />
                              <input className={FORM_INPUT_CLASS} type="number" placeholder="Weight" value={c.weight ?? ""} onChange={(e) => setComps((arr) => arr.map((y, j) => (j === i ? { ...y, weight: Number(e.target.value) } : y)))} />
                              <RowRemoveButton onClick={() => setComps((arr) => arr.filter((_, j) => j !== i))} />
                            </RepeatRow>
                          ))}
                        </div>
                      )}
                    </div>
                  ),
                },
              ]}
            />
            <StatusMessage formState={state} status={state?.status} message={state?.message} />
          </div>
        </Modal>
      )}
    </>
  );
}

export default memo(CareerPathSteps);
