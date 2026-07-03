import type { ReactNode } from "react";
import { useTranslation } from "react-i18next";
import Logo from "../headerLogo/logo";
function AuthHeader() {
  const { t } = useTranslation();

  return (
    <AuthHeaderShell>
      <span className="text-lg drop-shadow-sm font-serif">{t("AppName")}</span>
      <div className="flex items-center space-x-4">
        <Logo />
      </div>
    </AuthHeaderShell>
  );
}

function AuthHeaderShell({ children }: { children: ReactNode }) {
  return (
    <div className="pt-4 rounded-md text-black p-2 mb-2 font-bold flex justify-between items-center">
      {children}
    </div>
  );
}

export default AuthHeader;
