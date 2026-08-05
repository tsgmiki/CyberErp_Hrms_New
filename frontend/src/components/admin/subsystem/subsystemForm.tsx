"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { SubsystemModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveSubsystemService from "@/services/admin/subsystem/save";
import getAllSubsystems from "@/services/admin/subsystem/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

function SubsystemForm(props: { id: string; setSubsystemId: (id: string) => void }) {
  const { id, setSubsystemId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as SubsystemModel);

  // stale-form guard: when the id is cleared (back / Add-new) while this form stays
  // mounted, drop the previously loaded record so Add never shows stale values.
  useEffect(() => {
    if (!id) setFormData({} as SubsystemModel);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  // No GET /Subsystem/{id} — resolve the edited row from the (small) paged list.
  const { data: subsystems, isLoading: pending } = useQuery({
    queryKey: ["subsystems", "formLookup"],
    queryFn: () => getAllSubsystems({ ...parameterInitialData, take: 200 }),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveSubsystemService(formData);
    setFormState(result);
    setIsLoading(false);
  };

  const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  }, []);

  useEffect(() => {
    const row = subsystems?.data?.find((s) => s.id === id);
    if (row) setFormData(row);
  }, [subsystems, id]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as SubsystemModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["subsystems"] });
      setSubsystemId("");
    }
  }, [formState]);

  return (
    <div className=" text-white ">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 1,
          submitHandler: submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
          SubmitButton: "top",
          components: [
            {
              name: "name",
              label: "Name",
              placeholder: "e.g. HRMS",
              required: true,
              value: formData.name,
              onChange: changeHandler,
              error: formState?.zodErrors?.name,
              type: "text",
            },
            {
              name: "code",
              label: "Code",
              placeholder: "e.g. HRMS",
              required: true,
              value: formData.code,
              onChange: changeHandler,
              error: formState?.zodErrors?.code,
              type: "text",
            },
            {
              name: "sortOrder",
              label: "Sort Order",
              placeholder: "0",
              value: formData.sortOrder,
              onChange: changeHandler,
              error: formState?.zodErrors?.sortOrder,
              type: "text",
            },
            {
              name: "url",
              label: "Application URL",
              // NOTE: ':' and '.' are i18next separators — keep placeholders free of them.
              placeholder: "Where the subsystem app is hosted",
              value: formData.url,
              onChange: changeHandler,
              error: formState?.zodErrors?.url,
              type: "text",
            },
            {
              name: "id",
              value: formData.id,
              type: "hidden",
            },
          ],
        }}
      />
      <StatusMessage
        formState={formState}
        status={formState?.status}
        message={formState?.message}
      ></StatusMessage>
    </div>
  );
}
export default SubsystemForm;
