"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import type { MedicalContractModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import Loading from "../../common/loader/loader";
import saveMedicalContract from "@/services/admin/medicalContract/save";
import getMedicalContract from "@/services/admin/medicalContract/get";
import { getAllMedicalProviders } from "@/services/admin/medical";
import { parameterInitialData } from "@/constants/initialization";
import { optionLabel } from "@/constants/leave";

const FormProvider = memo(FormProviders);

const STATUS_OPTIONS = ["Active", "Expired", "Terminated"].map((x) => ({ id: x, name: x }));

const NEW_DEFAULTS: MedicalContractModel = {
  status: "Active",
};

function MedicalContractForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<MedicalContractModel>({ ...NEW_DEFAULTS });
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["medicalContract", id],
    queryFn: () => getMedicalContract(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const { data: providers, isLoading: providersLoading } = useQuery({
    queryKey: ["medicalProviderOptions"],
    queryFn: () => getAllMedicalProviders({ ...parameterInitialData, take: 200 }),
    staleTime: 60_000,
  });
  const providerOptions = (providers?.data ?? []).map((p) => ({ id: p.id!, name: p.name ?? "" }));

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveMedicalContract(fd);
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
      setFormData({ ...record, startDate: record.startDate?.slice(0, 10), renewalDate: record.renewalDate?.slice(0, 10), endDate: record.endDate?.slice(0, 10) });
    else if (!id) setFormData({ ...NEW_DEFAULTS });
  }, [record, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({ ...NEW_DEFAULTS });
      if (formRef.current) formRef.current.reset();
      queryClient.invalidateQueries({ queryKey: ["medicalContracts"] });
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
            { name: "medicalProviderId", label: "Provider", required: true, type: "dropDown", onSelect: selectHandler, value: formData.medicalProviderId, displayValue: optionLabel(providerOptions, formData.medicalProviderId) || formData.providerName, error: formState?.zodErrors?.medicalProviderId, isLoading: providersLoading, data: providerOptions as never },
            { name: "contractNumber", label: "Contract #", placeholder: "e.g. MC-2026-001", value: formData.contractNumber, onChange: changeHandler, type: "text" },
            { name: "status", label: "Status", type: "dropDown", onSelect: selectHandler, value: formData.status, displayValue: optionLabel(STATUS_OPTIONS, formData.status), data: STATUS_OPTIONS as never },
            { name: "startDate", label: "Start Date", type: "date", value: formData.startDate, onChange: changeHandler },
            { name: "renewalDate", label: "Renewal Date", type: "date", value: formData.renewalDate, onChange: changeHandler },
            { name: "endDate", label: "End Date", type: "date", value: formData.endDate, onChange: changeHandler },
            { name: "creditLimit", label: "Credit Limit", value: formData.creditLimit, onChange: changeHandler, inputType: "number", type: "text" },
            { name: "notes", label: "Notes", value: formData.notes, onChange: changeHandler, type: "text" },
            { name: "terms", label: "Terms", value: formData.terms, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}

export default MedicalContractForm;
