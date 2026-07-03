"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { OperationModel, ParameterModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveOperationService from "@/services/admin/operation/save";
import getOperation from "@/services/admin/operation/get";
import getAllModule from "@/services/admin/module/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

function OperationForm(props: {
  id: string;
  setOperationId: (id: string) => void;
  open?: boolean;
  onClose?: () => void;
}) {
  const { id, setOperationId, open = true, onClose } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as OperationModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: operation, isLoading: pending } = useQuery({
    queryKey: ["operation", id],
    queryFn: () => getOperation(id),
    enabled: typeof id != "undefined" && id != "",
  });
  const [param, setParam] = useState<ParameterModel>({ ...parameterInitialData });
  const { data: modules, isLoading: modulesLoading } = useQuery({
    queryKey: ["modules", param],
    queryFn: () => getAllModule(param),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveOperationService(formData);
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
    if (typeof operation != "undefined" && operation != null) {
      setFormData(operation);
    }
  }, [operation]);
  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as OperationModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["operations"] });
      setOperationId("");
      onClose?.();
    }
  }, [formState, onClose, queryClient, setOperationId]);

  return (
    <div className=" text-white ">
      {isLoading && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          showModal: true,
          modalVisible: open,
          onModalClose: () => {
            setOperationId("");
            onClose?.();
          },
          title: id ? "Edit Operation" : "Add Operation",
          modalSize: "lg",
          columnsNo: 2,
          submitHandler: submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading || pending,
          SubmitButton: "top",
          components: [
            {
              name: "moduleId",
              label: "Module",
              placeholder: "Module",
              required: true,
              value: formData.moduleId,
              displayValue: formData.module,
              error: formState?.zodErrors?.moduleId,
              type: "dropDown",
              setParam: setParam as any,
              param: param,
              isLoading: modulesLoading,
              onSelect: selectHandler,
              data: modules?.data?.map((item: any) => {
                return { id: item.id, name: item.name };
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
              name: "link",
              label: "Link",
              placeholder: "Link",
              required: true,
              value: formData.link,
              onChange: changeHandler,
              error: formState?.zodErrors?.link,
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
export default OperationForm;
