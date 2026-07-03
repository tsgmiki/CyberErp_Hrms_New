"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { memo, useCallback, useEffect, useState } from "react";
import type { UserRoleModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveUserRoleService from "@/services/admin/userRole/save";
import getUserRole from "@/services/admin/userRole/get";
import getAllRole from "@/services/admin/role/getAll";
import getAllUser from "@/services/admin/user/getAll";
import Loading from "../../common/loader/loader";
import { parameterInitialData } from "@/constants/initialization";

const FormProvider = memo(FormProviders);

function UserRoleForm(props: {
  id: string;
  setUserRoleId: (id: string) => void;
}) {
  const { id, setUserRoleId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as UserRoleModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: userRole, isLoading: pending } = useQuery({
    queryKey: ["userRole", id],
    queryFn: () => getUserRole(id),
    enabled: typeof id != "undefined" && id != "",
  });
  const [userParam, setUserParam] = useState({
    ...parameterInitialData,
  });
  const { data: users, isLoading: isUsersLoading } = useQuery({
    queryKey: ["users", userParam],
    queryFn: () => getAllUser(userParam),
  });
  const [roleParam, setRoleParam] = useState({
    ...parameterInitialData,
  });
  const { data: roles, isLoading: isRolesLoading } = useQuery({
    queryKey: ["roles", roleParam],
    queryFn: () => getAllRole(roleParam),
  });

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveUserRoleService(formData);
    setFormState(result);
    setIsLoading(false);
  };
  const selectHandler = useCallback((name: string, record: any) => {
    setFormData((prevState) => ({
      ...prevState,
      [name]: record.id,
    }));
  }, []);
  useEffect(() => {
    if (typeof userRole != "undefined" && userRole != null) {
      setFormData(userRole);
    }
  }, [userRole]);
  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as UserRoleModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["userRoles"] });
      setUserRoleId("");
    }
  }, [formState]);

  return (
    <div className=" text-white ">
      {pending && <Loading />}
      <FormProvider
        ref={formRef}
        form={{
          columnsNo: 2,
          submitHandler: submitHandler,
          labelWidth: "w-[30%]",
          isPending: isLoading,
           SubmitButton:'top',
          components: [
            {
              name: "userId",
              label: "User",
              placeholder: "User",
              required: true,
              value: formData.userId,
              displayValue: formData.user,
              error: formState?.zodErrors?.userId,
              type: "dropDown",
              param: userParam,
              setParam: setUserParam as any,
              isLoading: isUsersLoading,
              onSelect: selectHandler,
              data: users?.data?.map((item: any) => {
                return { id: item.id, name: item.fullName };
              }) as never,
            },

            {
              name: "roleId",
              label: "Role",
              placeholder: "Role",
              required: true,
              value: formData.roleId,
              displayValue: formData.role,
              error: formState?.zodErrors?.roleId,
              type: "dropDown",
              onSelect: selectHandler,
              param: roleParam,
              setParam: setRoleParam as any,
              isLoading: isRolesLoading,
              data: roles?.data?.map((item: any) => {
                return { id: item.id, name: item.name };
              }) as never,
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
export default UserRoleForm;
