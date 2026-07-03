"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { RoleModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import getRole from "@/services/admin/role/get";
import Loading from "../../common/loader/loader";
import saveRoleService from "@/services/admin/role/save";

const FormProvider = memo(FormProviders);

function RoleForm(props: { id: string; setRoleId: (id: string) => void }) {
  const { id, setRoleId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as RoleModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: role, isLoading: pending } = useQuery({
    queryKey: ["role", id],
    queryFn: () => getRole(id),
    enabled: typeof id != "undefined" && id != "",
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveRoleService(formData);
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
    if (typeof role != "undefined" && role != null) {
      setFormData(role);
    }
  }, [role]);
  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as RoleModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["roles"] });
      setRoleId("");
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
export default RoleForm;
