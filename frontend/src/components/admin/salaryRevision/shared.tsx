/** Shared display helpers for the salary-revision screens. */
export const money = (n?: number | null) => (n ?? 0).toLocaleString(undefined, { maximumFractionDigits: 2 });

export const revisionStatusBadge = (s?: string) =>
  ({
    Draft: "bg-secondary/40 text-muted",
    PendingApproval: "bg-warning/15 text-warning",
    Approved: "bg-info/15 text-info",
    Applied: "bg-success/15 text-success",
    Cancelled: "bg-muted/30 text-muted",
  }[s ?? ""] ?? "bg-muted/30 text-muted");
