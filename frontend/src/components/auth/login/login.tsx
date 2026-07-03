import FormProviders from "@/components/common/formProvider/formProvider";
import { StatusMessage } from "@/components/common/statusMessage/status";
import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import type { UserModel } from "@/models";
import loginService from "@/services/auth/login";
import { useAuth } from "@/context/AuthContext";

interface User {
  id: string;
  fullName: string;
  userName: string;
  email: string;
  isPublicUser: boolean;
  accessToken: string;
}

interface FormState {
  status?: string;
  message?: string;
  zodErrors?: Record<string, string[]>;
}

function LoginForm() {
  const [formState, setFormState] = useState<FormState>({
    status: "",
    message: "",
    zodErrors: {},
  });
  const [formData, setFormData] = useState({} as UserModel);
  const [isPending, setIsPending] = useState(false);
  const { login } = useAuth();
  const { t } = useTranslation();
  const navigate = useNavigate();

  const changeHandler = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData((prevState) => ({
      ...prevState,
      [name]: value,
    }));
  }, []);

  useEffect(() => {
    if (formState.status === "success") {
      navigate("/");
    }
  }, [formState.status, navigate]);

  const submitHandler = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const payload = new FormData(e.currentTarget);
    setIsPending(true);
    const result = await loginService(payload);

    if (result.status === "success" && result.user) {
      login(result.user as unknown as User);
    }

    setFormState(result);
    setIsPending(false);
  };

  return (
    <>
      <FormProviders
        form={{
          authMode: true,
          columnsNo: 1,
          isPending,
          submitBtnTitle: t("Sign In"),
          submitHandler,
          SubmitButton: "bottom",
          submitBtnClassName: "w-full h-11 justify-center text-on-accent font-display font-semibold tracking-wide shadow-sm hover:opacity-90",
          showLockOnSubmit: true,
          components: [
            {
              name: "userName",
              label: "User Name",
              placeholder: "User Name",
              required: true,
              layout: "auth",
              onChange: changeHandler,
              value: formData.userName,
              error: formState?.zodErrors?.userName,
              type: "text",
              colSpan: "full",
            },
            {
              name: "password",
              label: "Password",
              placeholder: "••••••••",
              required: true,
              layout: "auth",
              showPasswordToggle: true,
              value: formData.password,
              onChange: changeHandler,
              error: formState?.zodErrors?.password,
              type: "password",
              inputType: "password",
              colSpan: "full",
            },
          ],
        }}
      />
      <StatusMessage
        status={formState.status || ""}
        message={formState.message || ""}
        formState={formState}
      />
    </>
  );
}

export default LoginForm;
