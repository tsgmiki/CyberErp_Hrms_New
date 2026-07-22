"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { MedicalPlanModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveMedicalPlan from "@/services/admin/medicalPlan/save";
import getMedicalPlan from "@/services/admin/medicalPlan/get";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const NEW_DEFAULTS: MedicalPlanModel = {
  coveragePercent: 100,
  coversDependents: true,
  isActive: true,
};

function MedicalPlanForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<MedicalPlanModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["medicalPlan", id],
    queryFn: () => getMedicalPlan(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveMedicalPlan(fd);
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

  useEffect(() => {
    if (typeof record != "undefined" && record != null) setFormData(record);
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["medicalPlans"] });
      setId("");
    }
  }, [formState]);

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[40%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "name", label: "Name", placeholder: "e.g. Standard Cover", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "coveragePercent", label: "Coverage %", value: formData.coveragePercent, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "annualCoverageLimit", label: "Annual Limit (blank = ∞)", value: formData.annualCoverageLimit, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "coversDependents", label: "Covers Dependents", type: "dropDown", onSelect: selectHandler, value: boolId(formData.coversDependents), displayValue: yesNoLabel(formData.coversDependents), data: yesNoOptions as never },
            { name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive), data: yesNoOptions as never },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MedicalPlanForm;
