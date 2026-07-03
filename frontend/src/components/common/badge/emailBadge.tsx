import { Mail } from "lucide-react";
import IconBadge from "./iconBadge";

export interface EmailBadgeProps {
  value?: string | null;
  className?: string;
  link?: boolean;
}

function EmailBadge({ value, className = "", link = true }: EmailBadgeProps) {
  const email = value?.trim();
  const href = link && email ? `mailto:${email}` : undefined;

  return (
    <IconBadge
      value={email}
      icon={Mail}
      variant="info"
      className={className}
      href={href}
    />
  );
}

export default EmailBadge;
