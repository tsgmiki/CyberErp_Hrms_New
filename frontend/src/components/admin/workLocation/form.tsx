"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { WorkLocationModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveWorkLocation from "@/services/admin/workLocation/save";
import getWorkLocation from "@/services/admin/workLocation/get";
import getAllWorkLocation from "@/services/admin/workLocation/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { workLocationTypes, activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);

function WorkLocationForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as WorkLocationModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["workLocation", id],
    queryFn: () => getWorkLocation(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [parentParam, setParentParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: parents, isLoading: parentsLoading } = useQuery({
    queryKey: ["workLocations", parentParam],
    queryFn: () => getAllWorkLocation(parentParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveWorkLocation(fd);
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
  }, [record]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as WorkLocationModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["workLocations"] });
      setId("");
    }
  }, [formState]);

  const parentOptions = (parents?.data ?? [])
    .filter((l) => l.id !== formData.id)
    .map((l) => ({ id: l.id, name: `${l.name} (${l.locationType})` }));

  return (
    <div className="text-white">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            { name: "code", label: "Code", placeholder: "Code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
            { name: "name", label: "Name", placeholder: "Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "locationType", label: "Location Type", required: true, type: "dropDown", onSelect: selectHandler,
              value: formData.locationType, displayValue: formData.locationType,
              error: formState?.zodErrors?.locationType, data: workLocationTypes as never,
            },
            {
              name: "parentId", label: "Parent Location", type: "dropDown", onSelect: selectHandler,
              value: formData.parentId, displayValue: formData.parentName,
              param: parentParam, setParam: setParentParam as any, isLoading: parentsLoading,
              data: parentOptions as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "address", label: "Address", placeholder: "Address", value: formData.address, onChange: changeHandler, type: "text" },
            { name: "description", label: "Description", placeholder: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default WorkLocationForm;
