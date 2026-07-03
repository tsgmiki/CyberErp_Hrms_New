import {
  Building2,
  FolderKanban,
  Shield,
  Store,
  User,
  type LucideIcon,
} from "lucide-react";
import IconBadge from "./iconBadge";
import type { BadgeVariant } from "./badge";

export type EntityBadgeKind =
  | "user"
  | "customer"
  | "store"
  | "module"
  | "role"
  | "organization";

const entityConfig: Record<
  EntityBadgeKind,
  { icon: LucideIcon; variant: BadgeVariant }
> = {
  user: { icon: User, variant: "default" },
  customer: { icon: User, variant: "secondary" },
  store: { icon: Store, variant: "outline" },
  module: { icon: FolderKanban, variant: "info" },
  role: { icon: Shield, variant: "info" },
  organization: { icon: Building2, variant: "secondary" },
};

export interface EntityBadgeProps {
  value?: string | null;
  kind?: EntityBadgeKind;
  className?: string;
}

function EntityBadge({ value, kind = "user", className = "" }: EntityBadgeProps) {
  const { icon, variant } = entityConfig[kind];

  return (
    <IconBadge
      value={value}
      icon={icon}
      variant={variant}
      className={className}
    />
  );
}

export default EntityBadge;
