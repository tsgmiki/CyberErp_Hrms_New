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
import { useEntityRecord } from "@/template";
import RecordNotFound from "../../common/recordNotFound";

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

  // stale-form guard: when the id is cleared (back / Add-new) while this form stays
  // mounted, drop the previously loaded record so Add never shows stale values.
  useEffect(() => {
    if (!id) setFormData({} as BranchModel);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  // Shared by-id fetch (same ["branch", id] key as before, so the cache is unchanged).
  // `notFound` is the part that matters now that ids arrive from the URL: getBranch swallows a
  // 404 and resolves to undefined, which would otherwise render as an innocuous blank form.
  const { data: record, isLoading: pending, notFound } = useEntityRecord<BranchModel>(
    "branch",
    getBranch,
    { id },
  );

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

  // A stale/deleted id must not fall through to the form — see RecordNotFound for why.
  if (notFound) return <RecordNotFound onBack={() => setId("")} />;

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
            // Fall back to the route id: createSaveService picks PUT vs POST purely from whether
            // this is non-empty, so an unloaded record must NOT silently degrade into a create.
            { name: "id", value: formData.id ?? id, type: "hidden" },
          ],
        }}
      />
      <StatusMessage formState={formState} status={formState?.status} message={formState?.message} />
    </div>
  );
}
export default BranchForm;
