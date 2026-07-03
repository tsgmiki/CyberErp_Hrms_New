import type { ReactNode } from "react";
import AuthBrand from "./authBrand";

interface AuthLayoutProps {
  children: ReactNode;
  maxWidth?: "sm" | "md" | "lg";
  footer?: ReactNode;
}

const widthClasses = {
  sm: "max-w-md",
  md: "max-w-2xl",
  lg: "max-w-3xl",
};

function AuthLayout({ children, maxWidth = "sm", footer }: AuthLayoutProps) {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className={`w-full ${widthClasses[maxWidth]} p-8`}>
        <AuthBrand />
        {children}
        {footer}
      </div>
    </div>
  );
}

export default AuthLayout;
