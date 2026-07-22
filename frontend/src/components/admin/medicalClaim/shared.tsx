/** Shared display helpers for the HR medical-claim screens. */
export const money = (n?: number | null) =>
  (n ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

export const medicalStatusBadge = (s?: string) =>
  ({
    Pending: "bg-warning/15 text-warning",
    UnderReview: "bg-primary/15 text-primary",
    Approved: "bg-success/15 text-success",
    Rejected: "bg-error/15 text-error",
    Paid: "bg-secondary/40 text-foreground",
  }[s ?? ""] ?? "bg-muted/30 text-muted");
