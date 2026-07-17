"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { lazy, memo, useCallback, useEffect, useMemo, useState } from "react";
import React from "react";
import { ClipboardList, Users, GitBranch } from "lucide-react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { SuccessionPlanModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import { EntityFormTabs } from "@/components/common/tabs/entityFormTabs";
import saveSuccessionPlan from "@/services/admin/successionPlan/save";
import getSuccessionPlan from "@/services/admin/successionPlan/get";
import getAllCriticalPosition from "@/services/admin/criticalPosition/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { successionHorizonOptions, successionPlanStatusOptions } from "@/constants/careerDevelopment";

const FormProvider = memo(FormProviders);
const SuccessionChart = memo(lazy(() => import("./chart")));
const Candidates = memo(lazy(() => import("./candidates")));

function SuccessionPlanForm({ id, setId }: { id: string; setId: (id: string) => void }) {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as SuccessionPlanModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["successionPlan", id],
    queryFn: () => getSuccessionPlan(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: criticals } = useQuery({
    queryKey: ["criticalPositions", "planLookup"],
    queryFn: () => getAllCriticalPosition({ ...parameterInitialData, take: 200 }),
    staleTime: 60_000,
  });
  const criticalOptions = useMemo(
    () => (criticals?.data ?? []).map((c) => ({ id: c.id!, name: `${c.positionTitle ?? c.positionCode ?? "—"} (${c.riskLevel})` })),
    [criticals],
  );

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    setFormState(await saveSuccessionPlan(fd));
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => setFormData((p) => ({ ...p, [e.target.name]: e.target.value })), []);
  const selectHandler = useCallback((name: string, r: any) => setFormData((p) => ({ ...p, [name]: r.id })), []);

  useEffect(() => { if (record) setFormData(record); }, [record]);
  useEffect(() => {
    if (formState.status == "success") {
      queryClient.invalidateQueries({ queryKey: ["successionPlans"] });
      if (formState.id && !id) setId(formState.id);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formState]);

  return (
    <div className="space-y-4">
      {pending && <Loading />}
      <EntityFormTabs
        hasId={!!id}
        disabledHint="Save the plan to add ranked successors."
        tabs={[
          {
            key: "plan",
            label: "Succession Plan",
            Icon: ClipboardList,
            keepMounted: true,
            content: (
              <div className="space-y-4">
                <FormProvider
                  ref={formRef}
                  form={{
                    columnsNo: 2,
                    submitHandler,
                    labelWidth: "w-[30%]",
                    isPending: isLoading,
                    SubmitButton: "top",
                    formId: "successionPlanForm",
                    components: [
                      { name: "criticalPositionId", label: "Critical Position", required: true, type: "dropDown", onSelect: selectHandler,
                        value: formData.criticalPositionId, displayValue: formData.roleTitle, error: formState?.zodErrors?.criticalPositionId, data: criticalOptions as never },
                      { name: "name", label: "Plan Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
                      { name: "horizon", label: "Horizon", required: true, type: "dropDown", onSelect: selectHandler,
                        value: formData.horizon ?? "MediumTerm", displayValue: successionHorizonOptions.find((o) => o.id === (formData.horizon ?? "MediumTerm"))?.name, data: successionHorizonOptions as never },
                      { name: "status", label: "Status", required: true, type: "dropDown", onSelect: selectHandler,
                        value: formData.status ?? "Active", displayValue: successionPlanStatusOptions.find((o) => o.id === (formData.status ?? "Active"))?.name, data: successionPlanStatusOptions as never },
                      { name: "notes", label: "Notes", value: formData.notes, onChange: changeHandler, type: "textarea", colSpan: "full" },
                      { name: "id", value: formData.id, type: "hidden" },
                    ],
                  }}
                />
                <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
              </div>
            ),
          },
          { key: "successors", label: "Successors", Icon: Users, needsId: true, content: id ? <Candidates planId={id} /> : null },
          { key: "chart", label: "Succession Chart", Icon: GitBranch, needsId: true, content: id ? <SuccessionChart planId={id} /> : null },
        ]}
      />
    </div>
  );
}

export default memo(SuccessionPlanForm);
