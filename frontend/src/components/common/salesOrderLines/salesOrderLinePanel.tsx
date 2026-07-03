import type { ReactNode } from "react";

interface SalesOrderLinePanelProps {
  toolbar?: ReactNode;
  children: ReactNode;
}

/** Standard wrapper for sales order line tabs (details, bank, tax). */
export function SalesOrderLinePanel({ toolbar, children }: SalesOrderLinePanelProps) {
  return (
    <div className="overflow-hidden rounded-lg border border-border/80 bg-card">
      {toolbar}
      <div className="p-3 md:p-4">{children}</div>
    </div>
  );
}

export default SalesOrderLinePanel;
