import { Calendar } from "lucide-react";
import { formatBackendDate } from "@/components/util/dateFormater";
import Badge from "./badge";

export interface DateBadgeProps {
  value?: string | Date | null;
  showIcon?: boolean;
  className?: string;
}

function DateBadge({ value, showIcon = true, className = "" }: DateBadgeProps) {
  const label =
    value instanceof Date
      ? formatBackendDate(value.toISOString())
      : formatBackendDate(value ?? undefined);

  return (
    <Badge variant="outline" className={`tabular-nums ${className}`}>
      {showIcon ? <Calendar className="h-3 w-3 shrink-0 opacity-70" /> : null}
      <span className="truncate">{label}</span>
    </Badge>
  );
}

export default DateBadge;
