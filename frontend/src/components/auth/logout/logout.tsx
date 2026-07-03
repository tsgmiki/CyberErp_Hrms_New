import { StatusMessage } from "../../common/statusMessage/status";
import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import LogoutService from "@/services/auth/logout";
import Loading from "@/components/common/loader/loader";
import { useAuth } from "@/context/AuthContext";

interface FormState {
  status?: string;
  message?: string;
  zodErrors?: Record<string, string[]>;
}

function LogOutForm() {
  const [formState, setFormState] = useState<FormState>({
    status: "",
    message: "",
    zodErrors: {},
  });
  const [isPending, setIsPending] = useState(false);
  const { logout } = useAuth();

  const navigate = useNavigate();

  useEffect(() => {
    if (
      typeof formState.status != "undefined" &&
      formState.status == "success"
    ) {
      logout(); // Clear auth context state
      navigate("/login", { replace: true });
    }
  }, [formState.status, logout, navigate]);

  useEffect(() => {
    submitHandler();
  }, []);

  const submitHandler = async () => {
    setIsPending(true);
    const result = await LogoutService();
    setFormState(result);
    setIsPending(false);
  };

  return (
    <div className="  text-white p-1">
      {isPending && <Loading />}
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
export default LogOutForm;
