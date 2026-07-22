"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { AllowanceTypeModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveAllowanceType from "@/services/admin/allowanceType/save";
import getAllowanceType from "@/services/admin/allowanceType/get";
import { yesNoOptions, boolId, yesNoLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const CALC_OPTIONS = [
  { id: "Fixed", name: "Fixed amount" },
  { id: "PercentOfBase", name: "Percent of base" },
];

const NEW_DEFAULTS: AllowanceTypeModel = {
  calcMethod: "Fixed",
  isTaxable: true,
  isActive: true,
  sortOrder: 0,
};

function AllowanceTypeForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<AllowanceTypeModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["allowanceType", id],
    queryFn: () => getAllowanceType(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveAllowanceType(fd);
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
      queryClient.invalidateQueries({ queryKey: ["allowanceTypes"] });
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
            { name: "name", label: "Name", placeholder: "e.g. Transport Allowance", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            { name: "code", label: "Code", value: formData.code, onChange: changeHandler, type: "text" },
            { name: "calcMethod", label: "Calc Method", type: "select", value: formData.calcMethod, onChange: changeHandler, data: CALC_OPTIONS as never },
            { name: "defaultRate", label: "Default amount/percent", inputType: "number", value: formData.defaultRate, onChange: changeHandler, type: "text" },
            { name: "sortOrder", label: "Sort Order", inputType: "number", value: formData.sortOrder, onChange: changeHandler, type: "text" },
            { name: "isTaxable", label: "Taxable", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isTaxable), displayValue: yesNoLabel(formData.isTaxable), data: yesNoOptions as never },
            { name: "isActive", label: "Active", type: "dropDown", onSelect: selectHandler, value: boolId(formData.isActive), displayValue: yesNoLabel(formData.isActive), data: yesNoOptions as never },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default AllowanceTypeForm;
