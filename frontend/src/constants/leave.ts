// Attendance & Leave (HC030–HC052) — dropdown option sets + small helpers.

export const yesNoOptions = [
  { id: "true", name: "Yes" },
  { id: "false", name: "No" },
];

export const leaveAccrualOptions = [
  { id: "Annual", name: "Annual (up-front)" },
  { id: "Monthly", name: "Monthly accrual" },
  { id: "None", name: "None (always available)" },
];

export const genderEligibilityOptions = [
  { id: "Any", name: "Any" },
  { id: "Male", name: "Male only" },
  { id: "Female", name: "Female only" },
];

export const holidayTypeOptions = [
  { id: "Public", name: "Public" },
  { id: "Religious", name: "Religious" },
  { id: "Organizational", name: "Organizational" },
];

export const dayPartOptions = [
  { id: "Full", name: "Full day" },
  { id: "FirstHalf", name: "First half" },
  { id: "SecondHalf", name: "Second half" },
];

export const leaveStatusOptions = [
  { id: "", name: "All statuses" },
  { id: "Pending", name: "Pending" },
  { id: "Approved", name: "Approved" },
  { id: "Rejected", name: "Rejected" },
  { id: "Cancelled", name: "Cancelled" },
];

export const leaveStatusTone: Record<string, string> = {
  Pending: "bg-amber-500/15 text-amber-600",
  Approved: "bg-emerald-500/15 text-emerald-600",
  Rejected: "bg-rose-500/15 text-rose-600",
  Cancelled: "bg-slate-500/15 text-slate-500",
};

/** Bool → the "true"/"false" id used by FormProvider dropDowns + createSaveService booleanFields. */
export const boolId = (v: unknown): string => (v === true || v === "true" ? "true" : "false");
export const yesNoLabel = (v: unknown): string => (v === true || v === "true" ? "Yes" : "No");
/** Look up a display label for an enum-code value from an options list. */
export const optionLabel = (
  options: { id: string; name: string }[],
  value: unknown,
): string => options.find((o) => o.id === value)?.name ?? "";
