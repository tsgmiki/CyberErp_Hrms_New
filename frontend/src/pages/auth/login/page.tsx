import { Link } from "react-router-dom";
import { useTranslation } from "react-i18next";
import LoginForm from "@/components/auth/login/login";
import AuthLayout from "@/components/auth/authLayout/authLayout";

function LoginPage() {
  const { t } = useTranslation();

  return (
    <AuthLayout
      title={t("Sign in")}
      subtitle={t("Use your CyberERP account to access your workspace.")}
      footer={
        <p className="text-center text-xs text-muted-foreground">
          {t("Don't have an account?")}{" "}
          <Link to="/register" className="font-medium text-primary hover:underline">
            {t("Create account")}
          </Link>
        </p>
      }
    >
      <LoginForm />
    </AuthLayout>
  );
}

export default LoginPage;
