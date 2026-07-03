"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { ModuleModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveModuleService from "@/services/admin/module/save";
import getModule from "@/services/admin/module/get";
import Loading from "../../common/loader/loader";
import { subSystems } from "@/constants/subSystem";

const FormProvider = memo(FormProviders);

function ModuleForm(props: { id: string; setModuleId: (id: string) => void }) {
  const { id, setModuleId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as ModuleModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: module, isLoading: pending } = useQuery({
    queryKey: ["module", id],
    queryFn: () => getModule(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveModuleService(formData);
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
  const selectHandler = useCallback((name: string, record: any) => {
    setFormData((prevState) => ({
      ...prevState,
      [name]: record.id,
    }));
  }, []);
  useEffect(() => {
    if (typeof module != "undefined" && module != null) {
      setFormData(module);
    }
  }, [module]);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as ModuleModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["modules"] });
      setModuleId("");
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
          SubmitButton:'top',
          components: [
            {
              name: "subSystem",
              label: "Sub System",
              placeholder: "Sub System",
              required: true,
              value: formData.subSystem,
              displayValue: formData.subSystem,
              error: formState?.zodErrors?.subSystem,
              type: "dropDown",
              onSelect: selectHandler,
              data: subSystems?.map((item: any) => {
                return { id: item.name, name: item.name };
              }) as never,
            },
            {
              name: "name",
              label: "Name",
              placeholder: "Name",
              required: true,
              value: formData.name,
              onChange: changeHandler,
              error: formState?.zodErrors?.name,
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
export default ModuleForm;
