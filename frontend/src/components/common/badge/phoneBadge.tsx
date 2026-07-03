import { Phone } from "lucide-react";
import IconBadge from "./iconBadge";

export interface PhoneBadgeProps {
  value?: string | null;
  className?: string;
  link?: boolean;
}

function normalizeTel(value: string) {
  return value.replace(/\s+/g, "");
}

function PhoneBadge({ value, className = "", link = true }: PhoneBadgeProps) {
  const phone = value?.trim();
  const href = link && phone ? `tel:${normalizeTel(phone)}` : undefined;

  return (
    <IconBadge
      value={phone}
      icon={Phone}
      variant="outline"
      className={className}
      href={href}
    />
  );
}

export default PhoneBadge;
