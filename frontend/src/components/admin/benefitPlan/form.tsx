"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useQuery } from "@tanstack/react-query";
import type { BenefitPlanModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveBenefitPlan from "@/services/admin/benefitPlan/save";
import getBenefitPlan from "@/services/admin/benefitPlan/get";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const CATEGORY_OPTIONS = ["Health", "Life", "Disability", "Pension", "Other"].map((x) => ({ id: x, name: x }));
const METHOD_OPTIONS = [
  { id: "Fixed", name: "Fixed" },
  { id: "PercentOfBase", name: "% of base" },
];

const NEW_DEFAULTS: BenefitPlanModel = {
  category: "Health",
  employeeContributionMethod: "Fixed",
  employerContributionMethod: "Fixed",
  employeeContributionRate: 0,
  employerContributionRate: 0,
  isActive: true,
};

function BenefitPlanForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<BenefitPlanModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["benefitPlan", id],
    queryFn: () => getBenefitPlan(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveBenefitPlan(fd);
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
    if (typeof record != "undefined" && record != null)
      setFormData({ ...record, enrollmentOpenFrom: record.enrollmentOpenFrom?.slice(0, 10), enrollmentOpenTo: record.enrollmentOpenTo?.slice(0, 10) });
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["benefitPlans"] });
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
            { name: "name", label: "Name", placeholder: "e.g. Family Health Plan", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "category", label: "Category", type: "select", onChange: changeHandler, value: formData.category, data: CATEGORY_OPTIONS as never },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "text" },
            { name: "employeeContributionMethod", label: "Employee contribution", type: "select", onChange: changeHandler, value: formData.employeeContributionMethod, data: METHOD_OPTIONS as never },
            { name: "employeeContributionRate", label: "Employee rate/amount", inputType: "number", value: formData.employeeContributionRate, onChange: changeHandler, type: "text" },
            { name: "employerContributionMethod", label: "Employer contribution", type: "select", onChange: changeHandler, value: formData.employerContributionMethod, data: METHOD_OPTIONS as never },
            { name: "employerContributionRate", label: "Employer rate/amount", inputType: "number", value: formData.employerContributionRate, onChange: changeHandler, type: "text" },
            { name: "enrollmentOpenFrom", label: "Enrollment open from", value: formData.enrollmentOpenFrom, onChange: changeHandler, type: "date" },
            { name: "enrollmentOpenTo", label: "Enrollment open until", value: formData.enrollmentOpenTo, onChange: changeHandler, type: "date" },
            { name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive), data: yesNoOptions as never },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default BenefitPlanForm;
