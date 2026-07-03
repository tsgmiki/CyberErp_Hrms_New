import { memo } from "react";
import StatusCard from "./statusCard";
import type { StatusCardsProps } from "./types";

const columnClass: Record<NonNullable<StatusCardsProps["columns"]>, string> = {
  2: "grid-cols-1 sm:grid-cols-2",
  3: "grid-cols-1 sm:grid-cols-2 lg:grid-cols-3",
  4: "grid-cols-1 sm:grid-cols-2 lg:grid-cols-4",
  5: "grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5",
  6: "grid-cols-1 sm:grid-cols-2 md:grid-cols-3 xl:grid-cols-6",
};

function StatusCards({
  items,
  activeId,
  onSelect,
  className = "",
  isLoading,
  columns = 4,
}: StatusCardsProps) {
  if (!items.length && !isLoading) return null;

  if (isLoading) {
    return (
      <div className={`grid gap-2 ${columnClass[columns]} ${className}`}>
        {Array.from({ length: columns }).map((_, i) => (
          <div key={i} className="h-[4.25rem] animate-pulse rounded-lg bg-secondary" />
        ))}
      </div>
    );
  }

  return (
    <div className={`grid gap-2 ${columnClass[columns]} ${className}`}>
      {items.map((item) => {
        const filterable = item.filterable !== false && Boolean(onSelect);
        const isActive = activeId === item.id;

        return (
          <StatusCard
            key={item.id}
            title={item.title}
            value={item.value}
            subtitle={item.subtitle}
            icon={item.icon}
            variant={item.variant}
            isActive={isActive}
            onClick={
              filterable
                ? () => onSelect?.(item.id)
                : undefined
            }
          />
        );
      })}
    </div>
  );
}

export default memo(StatusCards);
