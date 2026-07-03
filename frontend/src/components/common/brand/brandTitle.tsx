import { useTranslation } from "react-i18next";

type BrandTitleSize = "sm" | "md" | "lg";

interface BrandTitleProps {
  className?: string;
  size?: BrandTitleSize;
}

const sizeClasses: Record<BrandTitleSize, string> = {
  sm: "text-[15px]",
  md: "text-3xl",
  lg: "text-4xl sm:text-5xl",
};

function BrandTitle({ className = "", size = "md" }: BrandTitleProps) {
  const { t } = useTranslation();

  return (
    <span
      className={`font-display font-bold tracking-tight text-foreground ${sizeClasses[size]} ${className}`}
    >
      {t("BrandPrefix")}
      <span className="text-primary">{t("BrandAccent")}</span>
    </span>
  );
}

export default BrandTitle;
