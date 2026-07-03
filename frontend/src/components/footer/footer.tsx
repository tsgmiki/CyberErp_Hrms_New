import { useTranslation } from "react-i18next";
import { useLocation } from "react-router-dom";

function Footer() {
  const location = useLocation();
  const pathName = location.pathname;
  const { t } = useTranslation();
  return (
    <>
      {!pathName.includes("/dashboard") && (
        <div className="flex justify-center gap-3 bg-[#030303] h-8 align-middle items-center text-white">
          <div className=" flex justify-between gap-4">
            <p className="footer-company-name">
              {t("CompanyName")} {t("AppName")} © 2026
            </p>
          </div>
        </div>
      )}
    </>
  );
}

export default Footer;
