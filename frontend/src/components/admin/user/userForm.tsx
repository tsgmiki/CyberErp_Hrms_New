"use client";
import FormProviders from "@/components/common/formProvider/formProvider";
import { lazy, memo, useCallback, useEffect, useState } from "react";
import type { UserModel } from "@/models";
import { StatusMessage } from "../../common/statusMessage/status";
import React from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import saveUserService from "@/services/admin/user/save";
import getUser from "@/services/admin/user/get";
import getAllEmployee from "@/services/admin/employee/getAll";
import { parameterInitialData } from "@/constants/initialization";
import Loading from "../../common/loader/loader";

const employeeLookup = { ...parameterInitialData, take: 200 };

import Tabs from "@/components/common/tabs";
const UserRole = memo(lazy(() => import("../userRole")));

const FormProvider = memo(FormProviders);

function UserForm(props: { id: string; setUserId: (id: string) => void }) {
  const { id, setUserId } = props;

  const [formState, setFormState] = useState<any>({});
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState({} as UserModel);
  const formRef = React.createRef<HTMLFormElement>();
  const queryClient = useQueryClient();

  const { data: user, isLoading: pending } = useQuery({
    queryKey: ["user", id],
    queryFn: () => getUser(id),
    enabled: typeof id != "undefined" && id != "",
  });

  // Employees — for linking this login account to an employee (owns the FK; drives branch scope).
  const { data: employees, isLoading: employeesLoading } = useQuery({
    queryKey: ["employees", employeeLookup],
    queryFn: () => getAllEmployee(employeeLookup),
  });
  const employeeOptions = [
    { id: "", name: "— None (system / head office) —" },
    ...(employees?.data ?? []).map((e) => ({ id: e.id, name: `${e.fullName ?? e.employeeNumber}` })),
  ];
  const linkedEmployeeName = employeeOptions.find((e) => e.id === formData.employeeId)?.name;
  const selectHandler = useCallback((name: string, r: any) => {
    setFormData((prev) => ({ ...prev, [name]: r.id }));
  }, []);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    setIsLoading(true);
    const result = await saveUserService(formData);
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
    if (typeof user != "undefined" && user != null) {
      setFormData(user);
    }
  }, [user]);
  useEffect(() => {
    if (formState.status == "success") {
      setFormData({} as UserModel);
      if (formRef.current) formRef?.current.reset();
      queryClient.invalidateQueries({ queryKey: ["users"] });
      setUserId("");
    }
  }, [formState]);

  const tab = [
    {
      id: 1,
      label: "User Role",
      content: id ? (
        <UserRole userId={id} />
      ) : (
        <span className=" items-center flex justify-center text-foreground">
          Please save user First
        </span>
      ),
    },
  ];
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
          SubmitButton: "top",
          components: [
            {
              name: "fullName",
              label: "Full Name",
              placeholder: "Full Name",
              required: true,
              value: formData.fullName,
              onChange: changeHandler,
              error: formState?.zodErrors?.fullName,
              type: "text",
            },
            {
              name: "phoneNumber",
              label: "Phone No",
              placeholder: "Phone No",
              required: true,
              value: formData.phoneNumber,
              onChange: changeHandler,
              error: formState?.zodErrors?.phoneNumber,
              type: "text",
            },
            {
              name: "email",
              label: "Email",
              placeholder: "Email",
              required: false,
              value: formData.email,
              onChange: changeHandler,
              error: formState?.zodErrors?.email,
              type: "text",
            },
            {
              name: "userName",
              label: "User Name",
              placeholder: "User Name",
              required: true,
              value: formData.userName,
              onChange: changeHandler,
              error: formState?.zodErrors?.userName,
              type: "text",
            },
            {
              name: "password",
              label: "Password",
              placeholder: "Password",
              required: !formData.id,
              value: formData.password,
              onChange: changeHandler,
              error: !formData.id && formState?.zodErrors?.password,
              type: "password",
            },
            {
              // The User owns the relationship to Employee; linking here scopes the user to that
              // employee's branch at login and enables evaluator permissions for them.
              name: "employeeId",
              label: "Linked Employee",
              type: "dropDown",
              onSelect: selectHandler,
              value: formData.employeeId,
              displayValue: linkedEmployeeName,
              isLoading: employeesLoading,
              data: employeeOptions as never,
            },

            {
              name: "id",
              value: formData.id,
              type: "hidden",
            },
          ],
        }}
      >
        <Tabs
          dir="top"
          tabs={tab}
          activeTab={1}
          className="col-span-full mt-2"
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
export default UserForm;
