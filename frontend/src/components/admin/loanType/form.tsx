"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { LoanTypeModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveLoanType from "@/services/admin/loanType/save";
import getLoanType from "@/services/admin/loanType/get";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const NEW_DEFAULTS: LoanTypeModel = {
  maxTermMonths: 12,
  interestRatePct: 0,
  minGuarantors: 0,
  serviceCommitmentMonths: 0,
  requiresGuarantor: false,
  isActive: true,
};

function LoanTypeForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<LoanTypeModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["loanType", id],
    queryFn: () => getLoanType(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveLoanType(fd);
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
      queryClient.invalidateQueries({ queryKey: ["loanTypes"] });
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
            { name: "name", label: "Name", placeholder: "e.g. Emergency Loan", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "text" },
            { name: "maxAmount", label: "Max Amount (blank = ∞)", value: formData.maxAmount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "maxSalaryMultiple", label: "Max × Salary", value: formData.maxSalaryMultiple, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "maxTermMonths", label: "Max Term (months)", value: formData.maxTermMonths, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "interestRatePct", label: "Interest % (flat, 0 = free)", value: formData.interestRatePct, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "requiresGuarantor", label: "Requires Guarantor", type: "dropDown", onSelect: selectHandler, value: boolId(formData.requiresGuarantor), displayValue: yesNoLabel(formData.requiresGuarantor), data: yesNoOptions as never },
            { name: "minGuarantors", label: "Min Guarantors", value: formData.minGuarantors, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "serviceCommitmentMonths", label: "Service Commitment (months)", value: formData.serviceCommitmentMonths, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive), data: yesNoOptions as never },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default LoanTypeForm;
