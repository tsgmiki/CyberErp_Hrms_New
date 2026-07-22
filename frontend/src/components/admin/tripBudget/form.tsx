"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { TripBudgetModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveTripBudget from "@/services/admin/tripBudget/save";
import getTripBudget from "@/services/admin/tripBudget/get";
import getAllOrganizationUnit from "@/services/admin/organizationUnit/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);
const ORG_WIDE = { id: "", name: "Organization-wide" };

const NEW_DEFAULTS: TripBudgetModel = {
  fiscalYear: new Date().getFullYear(),
};

function TripBudgetForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<TripBudgetModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["tripBudget", id],
    queryFn: () => getTripBudget(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: units, isLoading: unitsLoading } = useQuery({
    queryKey: ["organizationUnitOptions"],
    queryFn: () => getAllOrganizationUnit({ ...parameterInitialData, take: 300 }),
    staleTime: 60_000,
  });
  const unitOptions = [ORG_WIDE, ...(units?.data ?? []).map((u) => ({ id: u.id!, name: u.name! }))];

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveTripBudget(fd);
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
      queryClient.invalidateQueries({ queryKey: ["tripBudgets"] });
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
            { name: "fiscalYear", label: "Fiscal Year", required: true, value: formData.fiscalYear, onChange: changeHandler, error: formState?.zodErrors?.fiscalYear, inputType: "number", type: "text" },
            { name: "organizationUnitId", label: "Organization Unit", type: "dropDown", onSelect: selectHandler, value: formData.organizationUnitId ?? "", displayValue: formData.organizationUnitName ?? optionLabel(unitOptions, formData.organizationUnitId ?? ""), isLoading: unitsLoading, data: unitOptions as never },
            { name: "amount", label: "Amount", value: formData.amount, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "notes", label: "Notes", value: formData.notes, onChange: changeHandler, type: "text", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default TripBudgetForm;
