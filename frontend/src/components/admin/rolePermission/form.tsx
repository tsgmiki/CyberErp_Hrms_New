"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { lazy, memo, useCallback, useEffect, useState } from "react";
import type { RolePermissionModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { parameterInitialData } from "@/constants/initialization";
import getAllRole from "@/services/admin/role/getAll";
import saveRolePermissionService from "@/services/admin/rolePermission/save";
import type { PermissionState } from "./rolePermissionUtils";

const RolePermissionDetail = memo(lazy(() => import("./detail")));
const FormProvider = memo(FormProviders);

function RolePermissionForm() {
  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as RolePermissionModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

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
    const result = await saveRolePermissionService(formData);
    setFormState(result);
    setIsLoading(false);
  };
  const selectHandler = useCallback((name: string, record: any) => {
    setFormData((prevState) => ({
      ...prevState,
      [name]: record.id,
    }));
  }, []);
  const detailHandler = useCallback((details: PermissionState[]) => {
    setFormData((prevState) => ({
      ...prevState,
      details: details as unknown as RolePermissionModel["details"],
    }));
  }, []);

  useEffect(() => {
    if (formState.status == "success") {
      setFormData({
        details: [] as RolePermissionModel[],
      } as RolePermissionModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["rolePermissions"] });
    }
  }, [formState]);

  return (
    <div className=" text-white ">
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
              name: "id",
              value: formData.id,
              type: "hidden",
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
              name: "details",
              value: JSON.stringify(formData.details),
              type: "hidden",
            },
          ],
        }}
      >
        <RolePermissionDetail
          roleId={formData.roleId as string}
          editHandler={detailHandler}
          
        />
      </FormProvider>
      <StatusMessage
        formState={formState}
        status={formState?.status}
        message={formState?.message}
      ></StatusMessage>
    </div>
  );
}
export default RolePermissionForm;
