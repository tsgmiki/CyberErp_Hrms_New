/** Option lists for the Organizational Structure module (HRMS §3.1). */

export const organizationUnitTypes = [
  "BusinessUnit",
  "Directorate",
  "Division",
  "Department",
  "Team",
  "Branch"
].map((v) => ({ id: v, name: v }));

export const workLocationTypes = [
  "Country",
  "Region",
  "City",
  "Office",
].map((v) => ({ id: v, name: v }));

export const activeStatusOptions = [
  { id: "true", name: "Active" },
  { id: "false", name: "Inactive" },
];

/* ---- Employee Data Management (HRMS §3.2) ---- */

export const genderOptions = ["Male", "Female"].map((v) => ({ id: v, name: v }));

export const maritalStatusOptions = ["Single", "Married", "Divorced", "Widowed"].map((v) => ({
  id: v,
  name: v,
}));

export const employmentStatusOptions = [
  "Active",
  "Probation",
  "OnLeave",
  "Suspended",
  "Terminated",
  "Retired",
].map((v) => ({ id: v, name: v }));

export const relationshipOptions = [
  "Spouse",
  "Child",
  "Parent",
  "Sibling",
  "Other",
].map((v) => ({ id: v, name: v }));

export const fieldDataTypeOptions = ["Text", "Number", "Date", "Boolean", "Select"].map((v) => ({
  id: v,
  name: v,
}));

export const yesNoOptions = [
  { id: "true", name: "Yes" },
  { id: "false", name: "No" },
];

/** Normalizes a boolean/undefined isActive into the dropDown id string. */
export const activeId = (v?: boolean) => (v === false ? "false" : "true");
export const activeLabel = (v?: boolean) => (v === false ? "Inactive" : "Active");

/** Workflow-enabled HR processes (entityType keys of the generic engine). */
export const workflowEntityTypeOptions = [
  { id: "EmployeeMovement.Transfer", name: "Transfer" },
  { id: "EmployeeMovement.Promotion", name: "Promotion" },
  { id: "EmployeeMovement.Demotion", name: "Demotion" },
  { id: "DisciplinaryMeasure", name: "Disciplinary Measure" },
  { id: "EmployeeTermination", name: "Termination" },
];

/** Termination kinds. */
export const terminationTypeOptions = ["Voluntary", "Involuntary"].map((v) => ({
  id: v,
  name: v,
}));

export const workflowEntityTypeLabel = (id?: string) =>
  workflowEntityTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";

export const workflowStatusOptions = ["Running", "Approved", "Rejected"];

/** Personnel movement kinds (SAP-style actions). */
export const movementTypeOptions = ["Transfer", "Promotion", "Demotion"].map((v) => ({
  id: v,
  name: v,
}));

/** Disciplinary sanctions. */
export const measureTypeOptions = [
  { id: "VerbalWarning", name: "Verbal Warning" },
  { id: "WrittenWarning", name: "Written Warning" },
  { id: "FinalWarning", name: "Final Warning" },
  { id: "Suspension", name: "Suspension" },
  { id: "SalaryDeduction", name: "Salary Deduction" },
  { id: "Demotion", name: "Demotion" },
  { id: "Termination", name: "Termination" },
];

export const measureTypeLabel = (id?: string) =>
  measureTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";

export const disciplinaryStatusOptions = [
  { id: "Open", name: "Open" },
  { id: "UnderReview", name: "Under Review" },
  { id: "Resolved", name: "Resolved" },
  { id: "Cancelled", name: "Cancelled" },
];

export const disciplinaryStatusLabel = (id?: string) =>
  disciplinaryStatusOptions.find((o) => o.id === id)?.name ?? id ?? "";

/** HR document template kinds (HC022). Body is free-form HTML regardless of type. */
export const documentTypeOptions = [
  { id: "EmploymentLetter", name: "Employment Letter" },
  { id: "ExperienceLetter", name: "Experience Letter" },
  { id: "IdCard", name: "ID Card" },
  { id: "Other", name: "Other" },
];

export const documentTypeLabel = (id?: string) =>
  documentTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";
