import type { LucideIcon } from "lucide-react";
import type { BadgeVariant } from "../badge/badge";

export interface StatusCardItem {
  id: string;
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: LucideIcon;
  variant?: BadgeVariant;
  /** When false, card is display-only (no filter click). Default true. */
  filterable?: boolean;
}

export interface StatusCardsProps {
  items: StatusCardItem[];
  /** Highlights the active card (e.g. current `param.status` or `"all"`). */
  activeId?: string;
  onSelect?: (id: string) => void;
  className?: string;
  isLoading?: boolean;
  columns?: 2 | 3 | 4 | 5 | 6;
}
