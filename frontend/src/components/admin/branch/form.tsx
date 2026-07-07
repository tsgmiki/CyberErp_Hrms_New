"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { BranchModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveBranch from "@/services/admin/branch/save";
import getBranch from "@/services/admin/branch/get";
import getAllBranch from "@/services/admin/branch/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";
import { activeStatusOptions, activeId, activeLabel } from "@/constants/orgStructure";

const FormProvider = memo(FormProviders);
const yesNo = [
  { id: "true", name: "Yes" },
  { id: "false", name: "No" },
];

function BranchForm(props: { id: string; setId: (id: string) => void }) {
  const { id, setId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as BranchModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: record, isLoading: pending } = useQuery({
    queryKey: ["branch", id],
    queryFn: () => getBranch(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const [parentParam, setParentParam] = useState({ ...parameterInitialData, take: 100 });
  const { data: parents, isLoading: parentsLoading } = useQuery({
    queryKey: ["branches", parentParam],
    queryFn: () => getAllBranch(parentParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const fd = new FormData(e.target);
    setIsLoading(true);
    const result = await saveBranch(fd);
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
      setFormData({} as BranchModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["branches"] });
      setId("");
    }
  }, [formState]);

  const parentOptions = (parents?.data ?? [])
    .filter((b) => b.id !== formData.id)
    .map((b) => ({ id: b.id, name: b.name }));

  return (
    <div className="text-white">
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
            { name: "code", label: "Code", placeholder: "Code", required: true, value: formData.code, onChange: changeHandler, error: formState?.zodErrors?.code, type: "text" },
            { name: "name", label: "Name", placeholder: "Name", required: true, value: formData.name, onChange: changeHandler, error: formState?.zodErrors?.name, type: "text" },
            {
              name: "parentId", label: "Parent Branch", type: "dropDown", onSelect: selectHandler,
              value: formData.parentId, displayValue: formData.parentName,
              param: parentParam, setParam: setParentParam as any, isLoading: parentsLoading,
              data: parentOptions as never,
            },
            {
              name: "isHeadOffice", label: "Head Office", type: "dropDown", onSelect: selectHandler,
              value: formData.isHeadOffice === true ? "true" : "false",
              displayValue: formData.isHeadOffice === true ? "Yes" : "No",
              data: yesNo as never,
            },
            {
              name: "isActive", label: "Status", type: "dropDown", onSelect: selectHandler,
              value: activeId(formData.isActive), displayValue: activeLabel(formData.isActive),
              data: activeStatusOptions as never,
            },
            { name: "address", label: "Address", placeholder: "Address", value: formData.address, onChange: changeHandler, type: "text" },
            { name: "description", label: "Description", value: formData.description, onChange: changeHandler, type: "textarea", colSpan: "full" },
            { name: "id", value: formData.id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default BranchForm;
