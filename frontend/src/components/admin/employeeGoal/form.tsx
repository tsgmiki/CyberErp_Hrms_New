"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { Plus, Trash2, Save, Target, ListChecks } from "lucide-react";
import type { EmployeeGoalModel, GoalActionItemModel } from "@/models";
import { getEmployeeGoal, saveEmployeeGoal } from "@/services/admin/employeeGoal";
import EmployeePicker from "@/components/common/employeePicker";
import getAllReviewCycle from "@/services/admin/reviewCycle/getAll";
import getAllOrganizationalObjective from "@/services/admin/organizationalObjective/getAll";
import { goalStatusOptions } from "@/constants/performance";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import { parameterInitialData } from "@/constants/initialization";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 text-sm text-foreground focus:border-primary focus:outline-none";
const LABEL = "block text-xs font-medium text-muted mb-1";

interface EditableItem extends GoalActionItemModel {
  _key: number;
}

const num = (v: unknown): number | undefined => {
  if (v === "" || v === null || typeof v === "undefined") return undefined;
  const n = Number(v);
  return Number.isFinite(n) ? n : undefined;
};
const d10 = (v?: string) => (v ? v.slice(0, 10) : "");

const NEW_DEFAULTS: EmployeeGoalModel = { status: "Active", weight: 0, progressPercent: 0, setByManager: false };

function EmployeeGoalForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const keyCounter = useRef(0);
  const nextKey = () => ++keyCounter.current;

  const [meta, setMeta] = useState<EmployeeGoalModel>({ ...NEW_DEFAULTS });
  const [items, setItems] = useState<EditableItem[]>([]);
  const [formState, setFormState] = useState<any>({});
  const [isSaving, setIsSaving] = useState(false);

  const { data: record, isLoading } = useQuery({
    queryKey: ["employeeGoal", id],
    queryFn: () => getEmployeeGoal(id),
    enabled: id !== "",
  });

  const [cycleParam] = useState({ ...parameterInitialData, take: 200 });
  const { data: cycles } = useQuery({ queryKey: ["reviewCycles", cycleParam], queryFn: () => getAllReviewCycle(cycleParam) });
  const [objParam] = useState({ ...parameterInitialData, take: 500 });
  const { data: objectives } = useQuery({ queryKey: ["organizationalObjectives", objParam], queryFn: () => getAllOrganizationalObjective(objParam) });

  useEffect(() => {
    if (record) {
      setMeta({ ...record, startDate: d10(record.startDate), dueDate: d10(record.dueDate) });
      setItems((record.actionItems ?? []).map((a) => ({ ...a, dueDate: d10(a.dueDate), _key: nextKey() })));
    }
  }, [record]);

  const setMetaField = (name: keyof EmployeeGoalModel, value: unknown) =>
    setMeta((p) => ({ ...p, [name]: value }));

  const addItem = () =>
    setItems((p) => [...p, { _key: nextKey(), description: "", isCompleted: false, sortOrder: p.length }]);
  const updateItem = (key: number, patch: Partial<EditableItem>) =>
    setItems((p) => p.map((a) => (a._key === key ? { ...a, ...patch } : a)));
  const removeItem = (key: number) => setItems((p) => p.filter((a) => a._key !== key));

  const submit = async () => {
    setIsSaving(true);
    const payload: EmployeeGoalModel = {
      ...meta,
      organizationalObjectiveId: meta.organizationalObjectiveId || undefined,
      weight: num(meta.weight) ?? 0,
      progressPercent: num(meta.progressPercent) ?? 0,
      targetValue: num(meta.targetValue) ?? null,
      actionItems: items.map(({ _key, ...a }, i) => ({
        ...a,
        dueDate: a.dueDate || undefined,
        isCompleted: !!a.isCompleted,
        sortOrder: i,
      })),
    };
    const result = await saveEmployeeGoal(payload);
    setFormState(result);
    setIsSaving(false);
    if (result.status === "success") {
      queryClient.invalidateQueries({ queryKey: ["employeeGoals"] });
      setId("");
    }
  };

  if (isLoading) return <Loading />;

  const canSave = !!meta.employeeId && !!meta.reviewCycleId && !!meta.title && !!meta.startDate && !!meta.dueDate;

  return (
    <div className="space-y-4 text-foreground">
      <EntityFormTabs
        hasId={!!id}
        tabs={[
          {
            key: "details",
            label: "Goal Details",
            Icon: Target,
            description: "",//"Objective, alignment and measurable targets",
            keepMounted: true,
            content: (
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <div>
                  <label className={LABEL}>{t("Employee")} *</label>
                  {/* Role-scoped searchable picker: HR = all, manager = unit subtree, employee = locked to self. */}
                  <EmployeePicker
                    value={meta.employeeId}
                    displayValue={meta.employeeName}
                    onSelect={(eid, name) => setMeta((p) => ({ ...p, employeeId: eid, employeeName: name }))}
                  />
                </div>
                <div>
                  <label className={LABEL}>{t("Review Cycle")} *</label>
                  <select className={INPUT} value={meta.reviewCycleId ?? ""} onChange={(e) => setMetaField("reviewCycleId", e.target.value)}>
                    <option value="">{t("Select cycle")}</option>
                    {(cycles?.data ?? []).map((c) => (
                      <option key={c.id} value={c.id}>{c.name}</option>
                    ))}
                  </select>
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Title")} *</label>
                  <input className={INPUT} value={meta.title ?? ""} onChange={(e) => setMetaField("title", e.target.value)} placeholder="e.g. Close 20 enterprise deals" />
                </div>
                <div>
                  <label className={LABEL}>{t("Organizational Objective")}</label>
                  <select className={INPUT} value={meta.organizationalObjectiveId ?? ""} onChange={(e) => setMetaField("organizationalObjectiveId", e.target.value)}>
                    <option value="">{t("Unaligned")}</option>
                    {(objectives?.data ?? []).map((o) => (
                      <option key={o.id} value={o.id}>{o.title}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={LABEL}>{t("Status")}</label>
                  <select className={INPUT} value={meta.status ?? "Active"} onChange={(e) => setMetaField("status", e.target.value)}>
                    {goalStatusOptions.map((o) => (
                      <option key={o.id} value={o.id}>{t(o.name)}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className={LABEL}>{t("Measure")}</label>
                  <input className={INPUT} value={meta.measure ?? ""} onChange={(e) => setMetaField("measure", e.target.value)} placeholder={t("How success is measured") ?? ""} />
                </div>
                <div>
                  <label className={LABEL}>{t("Target Value")}</label>
                  <input type="number" step="any" className={INPUT} value={meta.targetValue ?? ""} onChange={(e) => setMetaField("targetValue", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Start Date")} *</label>
                  <input type="date" className={INPUT} value={meta.startDate ?? ""} onChange={(e) => setMetaField("startDate", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Due Date")} *</label>
                  <input type="date" className={INPUT} value={meta.dueDate ?? ""} onChange={(e) => setMetaField("dueDate", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Weight (%)")}</label>
                  <input type="number" step="any" className={INPUT} value={meta.weight ?? 0} onChange={(e) => setMetaField("weight", e.target.value)} />
                </div>
                <div>
                  <label className={LABEL}>{t("Progress (%)")}</label>
                  <input type="number" className={INPUT} value={meta.progressPercent ?? 0} onChange={(e) => setMetaField("progressPercent", e.target.value)} />
                </div>
                <div className="flex items-end gap-2 pb-1">
                  <input id="goal-mgr" type="checkbox" className="h-4 w-4 accent-primary" checked={meta.setByManager ?? false} onChange={(e) => setMetaField("setByManager", e.target.checked)} />
                  <label htmlFor="goal-mgr" className="text-sm">{t("Set by manager")}</label>
                </div>
                <div className="sm:col-span-2">
                  <label className={LABEL}>{t("Description")}</label>
                  <input className={INPUT} value={meta.description ?? ""} onChange={(e) => setMetaField("description", e.target.value)} />
                </div>
              </div>
            ),
          },
          {
            key: "action",
            label: "Action Plan",
            Icon: ListChecks,
            description: "Break the goal into trackable action items",
            keepMounted: true,
            content: (
              <div className="space-y-3">
                <div className="flex justify-end">
                  <button type="button" onClick={addItem} className="inline-flex items-center gap-1 rounded bg-primary px-2.5 py-1.5 text-xs font-semibold text-on-accent hover:opacity-90">
                    <Plus className="h-3.5 w-3.5" /> {t("Add Action Item")}
                  </button>
                </div>
                {items.length === 0 ? (
                  <p className="py-6 text-center text-sm text-muted">{t("No action items yet.")}</p>
                ) : (
                  <div className="space-y-2">
                    {items.map((a) => (
                      <div key={a._key} className="grid grid-cols-1 items-end gap-2 rounded-md border border-border/70 bg-secondary/20 p-2.5 md:grid-cols-[1fr_140px_90px_auto]">
                        <div>
                          <label className={LABEL}>{t("Description")} *</label>
                          <input className={INPUT} value={a.description ?? ""} onChange={(e) => updateItem(a._key, { description: e.target.value })} />
                        </div>
                        <div>
                          <label className={LABEL}>{t("Due Date")}</label>
                          <input type="date" className={INPUT} value={a.dueDate ?? ""} onChange={(e) => updateItem(a._key, { dueDate: e.target.value })} />
                        </div>
                        <div className="flex items-center gap-1 pb-2">
                          <input type="checkbox" className="h-4 w-4 accent-primary" checked={!!a.isCompleted} onChange={(e) => updateItem(a._key, { isCompleted: e.target.checked })} />
                          <label className="text-xs">{t("Done")}</label>
                        </div>
                        <div className="flex items-center pb-1">
                          <button type="button" onClick={() => removeItem(a._key)} className="rounded p-1 text-error hover:bg-error/10" title={t("Remove") ?? ""}>
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            ),
          },
        ]}
      />

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />

      {/* Persistent save bar — details + action plan save as one payload from any tab. */}
      <div className="flex items-center justify-end gap-2 border-t border-border pt-3">
        <button type="button" disabled={isSaving || !canSave} onClick={submit} className="inline-flex items-center gap-2 rounded-lg bg-primary px-5 py-2 text-sm font-semibold text-on-accent transition-colors hover:bg-primary-hover disabled:opacity-50">
          <Save className="h-4 w-4" /> {isSaving ? t("Saving…") : t("Save Goal")}
        </button>
      </div>
    </div>
  );
}

export default memo(EmployeeGoalForm);
