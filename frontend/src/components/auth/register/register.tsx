import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "../../common/statusMessage/status";
import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { RegisterUserModel } from "@/services/auth/register";
import registerService from "@/services/auth/register";
interface FormState {
  status?: string;
  message?: string;
  zodErrors?: Record<string, string[]>;
}

function RegisterForm() {
  const [formState, setFormState] = useState<FormState>({
    status: "",
    message: "",
    zodErrors: {},
  });
  const [formData, setFormData] = useState<RegisterUserModel>({});
  const [isPending, setIsPending] = useState(false);
  const { t } = useTranslation();
    const changeHandler = useCallback((e: any) => {
    const { name, value } = e.target;
    setFormData((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  }, []);
  const navigate = useNavigate();
  useEffect(() => {
    if (
      typeof formState.status != "undefined" &&
      formState.status == "success"
    ) {
      navigate("/login");
    }
  }, [formState]);

  const submitHandler = async (e: any) => {
    e.preventDefault();
    const formData = new FormData(e.target);
    console.log(
      "[RegisterForm] Raw FormData entries:",
      Object.fromEntries(formData),
    );
    setIsPending(true);
    const result = await registerService(formData);
    console.log("[RegisterForm] Service result:", result);
    setFormState(result);
    setIsPending(false);
  };

  return (
    <div className=" min-w-full">
      <FormProviders
        form={{
          authMode: true,
          columnsNo: 2,
          isPending: isPending,
          submitBtnTitle: t("Register"),
          submitHandler: submitHandler,
          SubmitButton: "bottom",
          submitBtnClassName: "w-full h-11 justify-center text-on-accent font-display font-semibold tracking-wide shadow-sm hover:opacity-90",
          components: [
            // Tenant Information Section
            {
              name: "companyInfo",
              label: "Company Information",
              type: "break",
            },
            {
              name: "tenantName",
              label: "Company",
              placeholder: "Company Name",
              required: true,
              onChange: changeHandler,
              value: formData.tenantName,
              error: formState?.zodErrors?.tenantName,
              type: "text",
              layout: "auth",
            },

            {
              name: "tenantAddress",
              label: "Address",
              placeholder: "Company Address (Optional)",
              required: false,
              onChange: changeHandler,
              value: formData.tenantAddress,
              error: formState?.zodErrors?.tenantAddress,
              type: "textarea",
            },

            {
              name: "accountInfo",
              label: "Account Information",
              type: "break",
            },
            // User Information Section
            {
              name: "fullName",
              label: "Full Name",
              placeholder: "Enter Your Full Name",
              required: true,
              onChange: changeHandler,
              value: formData.fullName,
              error: formState?.zodErrors?.fullName,
              type: "text",
              layout: "auth",
            },
            {
              name: "email",
              label: "Email",
              placeholder: "Enter Your Email",
              required: true,
              onChange: changeHandler,
              value: formData.email,
              error: formState?.zodErrors?.email,
              type: "text",
              inputType: "email",
              layout: "auth",
            },
            {
              name: "phoneNumber",
              label: "Phone No",
              placeholder: "Enter Your Phone Number",
              required: true,
              onChange: changeHandler,
              value: formData.phoneNumber,
              error: formState?.zodErrors?.phoneNumber,
              type: "text",
              layout: "auth",
            },
            {
              name: "userName",
              label: "User Name",
              placeholder: "Choose A Username",
              required: true,
              onChange: changeHandler,
              value: formData.userName,
              error: formState?.zodErrors?.userName,
              type: "text",
              layout: "auth",
            },
            {
              name: "password",
              label: "Password",
              placeholder: "Password",
              required: true,
              value: formData.password,
              onChange: changeHandler,
              error: formState?.zodErrors?.password,
              type: "password",
              inputType: "password",
              layout: "auth",
              showPasswordToggle: true,
            },
            {
              name: "confirmPassword",
              label: "Confirm Password",
              placeholder: "Confirm Password",
              required: true,
              value: formData.confirmPassword,
              onChange: changeHandler,
              error: formState?.zodErrors?.confirmPassword,
              type: "password",
              inputType: "password",
              layout: "auth",
              showPasswordToggle: true,
            },
          ],
        }}
      ></FormProviders>
      {
        <StatusMessage
          status={formState.status || ""}
          message={formState.message || ""}
          formState={formState}
        ></StatusMessage>
      }
    </div>
  );
}
export default RegisterForm;
