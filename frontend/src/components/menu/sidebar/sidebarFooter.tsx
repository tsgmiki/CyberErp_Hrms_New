import { useTranslation } from "react-i18next";

interface SidebarFooterProps {
  collapsed: boolean;
}

function SidebarFooter({ collapsed }: SidebarFooterProps) {
  const { t } = useTranslation();

  if (collapsed) return null;

  return (
    <div className="px-4 py-3 border-t border-sidebar-border shrink-0">
      <p className="text-[10px] text-muted-foreground/60 text-center">
        {t("FooterVersion", { defaultValue: "Cyber ERP v1.0" })}
      </p>
    </div>
  );
}

export default SidebarFooter;
