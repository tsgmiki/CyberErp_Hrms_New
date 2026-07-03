import { useTranslation } from "react-i18next";
import logo from "@/assets/logo.png";

function Logo() {
  const { t } = useTranslation();
  return (
    <div className="flex flex-col items-center">
      <img
        className="w-12 h-12 lg:w-24 lg:h-24 mx-auto"
        src={logo}
        alt="Logo"
      />
      <div className="text-black text-xl max-md:hidden font-serif text-center pt-2">
        {t("CompanyName")}
      </div>
    </div>
  );
}

export default Logo;

