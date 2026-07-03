import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import RegisterForm from "@/components/auth/register/register";
import AuthLayout from "@/components/auth/authLayout/authLayout";

function RegisterPage() {
  const { t } = useTranslation();

  return (
    <AuthLayout
      maxWidth="lg"
      footer={
        <p className="text-center text-xs text-muted-foreground mt-4">
          {t("Already have an account?")}{" "}
          <Link to="/login" className="font-medium text-primary hover:underline">
            {t("Sign in")}
          </Link>
        </p>
      }
    >
      <RegisterForm />
    </AuthLayout>
  );
}

export default RegisterPage;
