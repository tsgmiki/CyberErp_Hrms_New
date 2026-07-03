import Badge, { type BadgeVariant } from "./badge";

export interface MoneyBadgeProps {
  value?: number | string | null;
  currency?: string;
  locale?: string;
  variant?: BadgeVariant;
  className?: string;
}

function formatMoney(
  value: number | string | null | undefined,
  currency: string,
  locale: string,
): string {
  if (value === null || value === undefined || value === "") return "-";

  const numeric = typeof value === "string" ? Number(value.replace(/,/g, "")) : value;
  if (Number.isNaN(numeric)) return String(value);

  try {
    return new Intl.NumberFormat(locale, {
      style: "currency",
      currency,
      minimumFractionDigits: 2,
      maximumFractionDigits: 2,
    }).format(numeric);
  } catch {
    return `${numeric.toLocaleString(locale)} ${currency}`;
  }
}

function MoneyBadge({
  value,
  currency = "ETB",
  locale = "en-ET",
  variant = "success",
  className = "",
}: MoneyBadgeProps) {
  const label = formatMoney(value ?? null, currency, locale);

  return (
    <Badge variant={variant} className={`tabular-nums font-semibold ${className}`}>
      {label}
    </Badge>
  );
}

export default MoneyBadge;
