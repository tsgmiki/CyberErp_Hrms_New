"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { WorkflowDefinitionModel, WorkflowStepModel, WorkflowApproverModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { X, UserRound, Shield, GitBranch } from "lucide-react";
import { saveWorkflowDefinition, getWorkflowDefinition } from "@/services/admin/workflow";
import getAllUser from "@/services/admin/user/getAll";
import getAllRole from "@/services/admin/role/getAll";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import { EMPTY_APPROVER_ID } from "@/models/masters/HrWorkflowModel";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import {
  activeStatusOptions,
  activeId,
  activeLabel,
  workflowEntityTypeOptions,
  workflowEntityTypeLabel,
} from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

const MAX_STEPS = 10;
const lookupParam = { ...parameterInitialData, take: 100 };

interface StepDraft {
  name: string;
  approvers: WorkflowApproverModel[];
}

/** Resizes the step list to `count`, preserving already-configured steps. */
function resizeSteps(steps: StepDraft[], count: number): StepDraft[] {
  const next = steps.slice(0, count);
  while (next.length < count) next.push({ name: "", approvers: [] });
  return next;
}

function WorkflowDefinitionForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;
  const { t } = useTranslation();

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<WorkflowDefinitionModel>({ isActive: true });
  const [steps, setSteps] = useState<StepDraft[]>([{ name: "", approvers: [] }]);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["workflowDefinition", id],
    queryFn: () => getWorkflowDefinition(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: users } = useQuery({
    queryKey: ["users", lookupParam],
    queryFn: () => getAllUser(lookupParam),
  });
  const { data: roles } = useQuery({
    queryKey: ["roles", lookupParam],
    queryFn: () => getAllRole(lookupParam),
  });
  const { data: orgUnits } = useQuery({
    queryKey: ["organizationUnits", lookupParam],
    queryFn: () => getAllOrganizationUnit(lookupParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    setIsLoading(true);
    const payload: WorkflowStepModel[] = steps.map((s, i) => ({
      stepOrder: i + 1,
      name: s.name.trim(),
      approvers: s.approvers,
    }));
    const result = await saveWorkflowDefinition({ ...formData, steps: payload });
    setFormState(result);
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((p) => ({ ...p, [name]: value }));
  }, []);
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((p) => ({ ...p, [name]: r.id }));
  }, []);

  const setStepName = (index: number, name: string) =>
    setSteps((p) => p.map((s, i) => (i === index ? { ...s, name } : s)));

  const addApprover = (index: number, approver: WorkflowApproverModel) =>
    setSteps((p) =>
      p.map((s, i) =>
        i === index &&
        !s.approvers.some(
          (a) => a.approverType === approver.approverType && a.approverId === approver.approverId,
        )
          ? { ...s, approvers: [...s.approvers, approver] }
          : s,
      ),
    );

  const removeApprover = (index: number, approver: WorkflowApproverModel) =>
    setSteps((p) =>
      p.map((s, i) =>
        i === index
          ? {
              ...s,
              approvers: s.approvers.filter(
                (a) => !(a.approverType === approver.approverType && a.approverId === approver.approverId),
              ),
            }
          : s,
      ),
    );

  useEffect(() => {
    if (typeof record != "undefined" && record != null) {
      setFormData(record);
      setSteps(
        (record.steps ?? []).map((s) => ({
          name: s.name,
          approvers: (s.approvers ?? []).map((a) => ({
            approverType: a.approverType,
            approverId: a.approverId,
            displayName: a.displayName,
          })),
        })),
      );
    }
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ isActive: true });
      setSteps([{ name: "", approvers: [] }]);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["workflowDefinitions"] });
      setId("");
    }
  }, [formState]);

  const selectClass =
    "h-8 rounded-md border border-border bg-background px-2 text-xs text-foreground";

  return (
    <div className="text-foreground">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[35%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "name", label: "Workflow Name", placeholder: "e.g. Transfer Approval",
              required: true, value: formData.name, onChange: changeHandler,
              error: formState?.zodErrors?.name, type: "text",
            },
            {
              name: "entityType", label: "Process", required: true, type: "dropDown",
              onSelect: selectHandler, value: formData.entityType,
              displayValue: workflowEntityTypeLabel(formData.entityType),
              error: formState?.zodErrors?.entityType, data: workflowEntityTypeOptions as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            {
              name: "description", label: "Description", placeholder: "Optional note",
              value: formData.description, onChange: changeHandler, type: "textarea",
            },
          ],
        }}
      />

      {/* Approval chain designer */}
      <div className="mt-4 rounded-lg border border-border bg-card p-4">
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <div>
            <h4 className="text-sm font-semibold text-foreground">{t("Approval Steps")}</h4>
            <p className="text-xs text-muted-foreground">
              {t("Assign each step to users and/or roles. A step with no approvers is open to any user.")}
            </p>
          </div>
          <label className="flex items-center gap-2 text-xs font-medium text-foreground">
            {t("Number of Steps")}
            <input
              type="number"
              min={1}
              max={MAX_STEPS}
              value={steps.length}
              onChange={(e) => {
                const n = Math.max(1, Math.min(MAX_STEPS, Number(e.target.value) || 1));
                setSteps((p) => resizeSteps(p, n));
              }}
              className="h-9 w-16 rounded-lg border border-border bg-background px-2 text-center text-sm text-foreground"
            />
          </label>
        </div>

        {formState?.zodErrors?.steps && (
          <p className="mb-2 text-xs text-error">{formState.zodErrors.steps[0]}</p>
        )}

        <ol className="space-y-2">
          {steps.map((step, i) => (
            <li key={i} className="rounded-md border border-border/70 bg-background/40 p-3">
              <div className="flex flex-wrap items-center gap-2">
                <span className="flex h-6 w-6 shrink-0 items-center justify-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                  {i + 1}
                </span>
                <input
                  type="text"
                  value={step.name}
                  onChange={(e) => setStepName(i, e.target.value)}
                  placeholder={`${t("Step")} ${i + 1} — ${t("name")}`}
                  className="h-9 min-w-48 flex-1 rounded-lg border border-border bg-background px-3 text-sm text-foreground"
                />
                <select
                  className={selectClass}
                  value=""
                  onChange={(e) => {
                    const u = (users?.data ?? []).find((x) => x.id === e.target.value);
                    if (u?.id) addApprover(i, { approverType: "User", approverId: u.id, displayName: u.fullName });
                  }}
                >
                  <option value="">{t("+ Add user approver")}</option>
                  {(users?.data ?? []).map((u) => (
                    <option key={u.id} value={u.id}>{u.fullName}</option>
                  ))}
                </select>
                <select
                  className={selectClass}
                  value=""
                  onChange={(e) => {
                    const r = (roles?.data ?? []).find((x) => x.id === e.target.value);
                    if (r?.id) addApprover(i, { approverType: "Role", approverId: r.id, displayName: r.name });
                  }}
                >
                  <option value="">{t("+ Add role approver")}</option>
                  {(roles?.data ?? []).map((r) => (
                    <option key={r.id} value={r.id}>{r.name}</option>
                  ))}
                </select>
                <select
                  className={selectClass}
                  value=""
                  onChange={(e) => {
                    // Dynamic / self approvers resolve per request from the org structure at decision time.
                    if (e.target.value === "__immediate__") {
                      addApprover(i, { approverType: "ImmediateManager", approverId: EMPTY_APPROVER_ID, displayName: "Immediate Manager" });
                      return;
                    }
                    if (e.target.value === "__second__") {
                      addApprover(i, { approverType: "SecondLevelManager", approverId: EMPTY_APPROVER_ID, displayName: "Second-Level Manager" });
                      return;
                    }
                    if (e.target.value === "__subject__") {
                      addApprover(i, { approverType: "Subject", approverId: EMPTY_APPROVER_ID, displayName: "Subject (the employee)" });
                      return;
                    }
                    const u = (orgUnits?.data ?? []).find((x) => x.id === e.target.value);
                    if (u?.id)
                      addApprover(i, {
                        approverType: "UnitManager",
                        approverId: u.id,
                        displayName: `Manager of ${u.name}`,
                      });
                  }}
                >
                  <option value="">{t("+ Add dynamic approver")}</option>
                  <option value="__subject__">{t("Subject (the employee themselves)")}</option>
                  <option value="__immediate__">{t("Immediate Manager (requester's chain)")}</option>
                  <option value="__second__">{t("Second-Level Manager (manager's manager)")}</option>
                  {(orgUnits?.data ?? []).map((u) => (
                    <option key={u.id} value={u.id}>{t("Manager of")} {u.name}</option>
                  ))}
                </select>
              </div>

              {/* Approver chips */}
              <div className="mt-2 flex flex-wrap gap-1.5 pl-8">
                {step.approvers.length === 0 && (
                  <span className="text-[11px] italic text-muted">{t("Open step — any user may act")}</span>
                )}
                {step.approvers.map((a) => (
                  <span
                    key={`${a.approverType}:${a.approverId}`}
                    className={`inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs ${
                      a.approverType === "Role"
                        ? "border-info/40 bg-info/10 text-info"
                        : a.approverType === "ImmediateManager" || a.approverType === "UnitManager" || a.approverType === "SecondLevelManager"
                          ? "border-warning/40 bg-warning/10 text-warning"
                          : "border-primary/40 bg-primary/10 text-primary"
                    }`}
                  >
                    {a.approverType === "Role" ? (
                      <Shield size={11} />
                    ) : a.approverType === "ImmediateManager" || a.approverType === "UnitManager" || a.approverType === "SecondLevelManager" ? (
                      <GitBranch size={11} />
                    ) : (
                      <UserRound size={11} />
                    )}
                    {a.displayName}
                    <button
                      type="button"
                      onClick={() => removeApprover(i, a)}
                      className="ml-0.5 rounded-full hover:opacity-70"
                      aria-label={t("Remove")}
                    >
                      <X size={11} />
                    </button>
                  </span>
                ))}
              </div>
            </li>
          ))}
        </ol>
      </div>

      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default WorkflowDefinitionForm;
