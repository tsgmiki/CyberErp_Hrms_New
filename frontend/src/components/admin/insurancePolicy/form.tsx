"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { InsurancePolicyModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveInsurancePolicy from "@/services/admin/insurancePolicy/save";
import getInsurancePolicy from "@/services/admin/insurancePolicy/get";
import getAllInsurancePolicies from "@/services/admin/insurancePolicy/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { yesNoOptions, boolId, yesNoLabel, optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const TYPE_OPTIONS = ["Life", "Health", "Disability", "Accident", "WorkersCompensation", "Other"].map((x) => ({ id: x, name: x }));
const FREQUENCY_OPTIONS = ["Annual", "SemiAnnual", "Quarterly", "Monthly"].map((x) => ({ id: x, name: x }));

const NEW_DEFAULTS: InsurancePolicyModel = {
  insuranceType: "Health",
  premiumFrequency: "Annual",
  policyYear: new Date().getFullYear(),
  isRenewal: false,
};

function InsurancePolicyForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<InsurancePolicyModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["insurancePolicy", id],
    queryFn: () => getInsurancePolicy(id),
    enabled: typeof id != "undefined" && id != "",
  });

  // Prior policies for the renewal combobox.
  const { data: policies, isLoading: policiesLoading } = useQuery({
    queryKey: ["insurancePolicyOptions"],
    queryFn: () => getAllInsurancePolicies({ ...parameterInitialData, take: 200 }),
    staleTime: 60_000,
  });
  const priorOptions = (policies?.data ?? [])
    .filter((p) => p.id !== id)
    .map((p) => ({ id: p.id!, name: `${p.policyNumber} — ${p.insurerName}` }));

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveInsurancePolicy(fd);
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
      setFormData({ ...record, startDate: record.startDate?.slice(0, 10), endDate: record.endDate?.slice(0, 10) });
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["insurancePolicies"] });
      queryClient.invalidateQueries({ queryKey: ["insurancePolicyOptions"] });
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
            { name: "policyNumber", label: "Policy Number", placeholder: "e.g. POL-2026-001", required: true, value: formData.policyNumber, onChange: changeHandler, error: formState?.zodErrors?.policyNumber, type: "text" },
            { name: "insurerName", label: "Insurer", placeholder: "e.g. Nyala Insurance", required: true, value: formData.insurerName, onChange: changeHandler, error: formState?.zodErrors?.insurerName, type: "text" },
            { name: "insuranceType", label: "Insurance Type", type: "dropDown", onSelect: selectHandler, value: formData.insuranceType, displayValue: optionLabel(TYPE_OPTIONS, formData.insuranceType), data: TYPE_OPTIONS as never },
            { name: "policyYear", label: "Policy Year", value: formData.policyYear, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "startDate", label: "Start Date", required: true, type: "date", value: formData.startDate, onChange: changeHandler, error: formState?.zodErrors?.startDate },
            { name: "endDate", label: "End Date", required: true, type: "date", value: formData.endDate, onChange: changeHandler, error: formState?.zodErrors?.endDate },
            { name: "coverageAmount", label: "Coverage Amount", value: formData.coverageAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "annualPremium", label: "Annual Premium", value: formData.annualPremium, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "premiumFrequency", label: "Premium Frequency", type: "dropDown", onSelect: selectHandler, value: formData.premiumFrequency, displayValue: optionLabel(FREQUENCY_OPTIONS, formData.premiumFrequency), data: FREQUENCY_OPTIONS as never },
            { name: "coverage", label: "Coverage", placeholder: "What the policy covers", value: formData.coverage, onChange: changeHandler, type: "text" },
            { name: "isRenewal", label: "Is Renewal", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isRenewal), displayValue: yesNoLabel(formData.isRenewal), data: yesNoOptions as never },
            { name: "previousPolicyId", label: "Renews Policy", type: "dropDown", onSelect: selectHandler, value: formData.previousPolicyId, displayValue: optionLabel(priorOptions, formData.previousPolicyId), isLoading: policiesLoading, data: priorOptions as never },
            { name: "notes", label: "Notes", value: formData.notes, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default InsurancePolicyForm;
