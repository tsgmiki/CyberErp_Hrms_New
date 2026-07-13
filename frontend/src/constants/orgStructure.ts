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

export const employmentNatureOptions = [
  { id: "Permanent", name: "Permanent" },
  { id: "Contract", name: "Contract" },
];

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

/** Field types for the dynamic Form Builder — adds "Attachment" (file uploads), which HC021 custom
 * fields do not support. */
export const dynamicFormFieldTypeOptions = [
  ...fieldDataTypeOptions,
  { id: "Attachment", name: "Attachment (files)" },
];

/** Forms a custom field (HC021) can apply to. `id` matches the backend EmployeeFieldOwnerType enum;
 * `name` is the friendly label (the "Dependent" owner shows as "Family"). */
export const fieldOwnerTypeOptions = [
  { id: "Employee", name: "Employee" },
  { id: "Education", name: "Education" },
  { id: "Experience", name: "Experience" },
  { id: "Dependent", name: "Family" },
  { id: "Movement", name: "Movement" },
  { id: "Discipline", name: "Discipline" },
  { id: "Termination", name: "Termination" },
];
export const ownerTypeLabel = (id?: string) =>
  fieldOwnerTypeOptions.find((o) => o.id === id)?.name ?? id ?? "Employee";

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
  { id: "LeaveRequest", name: "Leave Request" },
  { id: "WorkforcePlan", name: "Workforce Plan" },
  { id: "HiringRequest", name: "Hiring Need" },
  { id: "JobRequisition", name: "Job Requisition" },
  { id: "JobOffer", name: "Job Offer" },
];

/** Recruitment levels a screening criterion can be scoped to (empty id = all steps). */
export const criterionStageOptions = [
  { id: "", name: "All Steps" },
  { id: "Screening", name: "Screening" },
  { id: "Interview", name: "Interview" },
  { id: "Selected", name: "Final Review" },
];

/** Interview round formats (HC101). */
export const interviewFormatOptions = [
  { id: "InPerson", name: "In Person" },
  { id: "Video", name: "Video Call" },
  { id: "Phone", name: "Phone" },
  { id: "Panel", name: "Panel" },
  { id: "TechnicalTest", name: "Technical Test" },
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
  { id: "ClearanceCertificate", name: "Clearance Certificate" },
  { id: "Other", name: "Other" },
];

export const documentTypeLabel = (id?: string) =>
  documentTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";

/** Workforce-plan horizons (HC053). */
export const planHorizonOptions = [
  { id: "Annual", name: "Annual (1 year)" },
  { id: "MediumTerm", name: "Medium-term (2–3 years)" },
  { id: "MultiYear", name: "Multi-year (4+ years)" },
];

export const planHorizonLabel = (id?: string) =>
  planHorizonOptions.find((o) => o.id === id)?.name ?? id ?? "";

/** Workforce-plan scenarios (HC067). */
export const planScenarioOptions = [
  { id: "Baseline", name: "Baseline" },
  { id: "Growth", name: "Growth" },
  { id: "Contraction", name: "Contraction" },
  { id: "Restructuring", name: "Restructuring" },
];

/** Planning-level employment types (HC057) — broader than the employee master's nature. */
export const plannedEmploymentTypeOptions = [
  { id: "Permanent", name: "Permanent" },
  { id: "Contract", name: "Contract" },
  { id: "Intern", name: "Intern" },
  { id: "Consultant", name: "Consultant" },
];

/** Candidate acquisition sources (HC092). */
export const candidateSourceOptions = [
  { id: "External", name: "External" },
  { id: "Internal", name: "Internal (Employee)" },
  { id: "JobBoard", name: "Job Board" },
  { id: "SocialMedia", name: "Social Media" },
  { id: "Referral", name: "Referral" },
  { id: "WalkIn", name: "Walk-in" },
];

/** Posting channels (HC088). */
export const postingChannelOptions = [
  { id: "Internal", name: "Internal Job Market" },
  { id: "External", name: "External Portal" },
  { id: "Both", name: "Internal + External" },
];

/** Application pipeline stages HR can move to (HC098; offer/hire stages are process-driven). */
export const applicationStageOptions = [
  { id: "Received", name: "Received" },
  { id: "Screening", name: "Screening" },
  { id: "Shortlisted", name: "Shortlisted" },
  { id: "Interview", name: "Interview" },
  { id: "Selected", name: "Selected" },
  { id: "Rejected", name: "Rejected" },
  { id: "Withdrawn", name: "Withdrawn" },
];

/** Who evaluates a screening criterion. */
export const criterionEvaluatorTypeOptions = [
  { id: "None", name: "HR / Unassigned" },
  { id: "Employee", name: "Internal Employee" },
  { id: "ExternalPerson", name: "External Person" },
  { id: "Organization", name: "Organization" },
];

/** Candidate document kinds; the first four + a signed offer/contract are the compliance set. */
export const candidateDocumentTypeOptions = [
  { id: "NationalId", name: "National ID" },
  { id: "GuarantorForm", name: "Guarantor Form" },
  { id: "MedicalCertificate", name: "Medical Certificate" },
  { id: "SignedOfferLetter", name: "Signed Offer Letter" },
  { id: "EmploymentContract", name: "Employment Contract" },
  { id: "EducationCertificate", name: "Education Certificate" },
  { id: "ExperienceLetter", name: "Experience Letter" },
  { id: "Other", name: "Other" },
];

export const candidateDocumentTypeLabel = (id?: string) =>
  candidateDocumentTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";
