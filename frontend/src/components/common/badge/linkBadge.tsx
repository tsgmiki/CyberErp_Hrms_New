import { ExternalLink, Link2 } from "lucide-react";
import IconBadge from "./iconBadge";

export interface LinkBadgeProps {
  value?: string | null;
  className?: string;
  external?: boolean;
}

function LinkBadge({ value, className = "", external = false }: LinkBadgeProps) {
  const href = value?.trim();
  const isExternal =
    external || (href ? /^https?:\/\//i.test(href) : false);

  return (
    <IconBadge
      value={href}
      icon={isExternal ? ExternalLink : Link2}
      variant="default"
      className={className}
      href={href && href !== "-" ? href : undefined}
    />
  );
}

export default LinkBadge;
