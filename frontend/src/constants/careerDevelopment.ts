/** Dropdown option sets for the Career Development §3.7.A screens (values = the backend enum names). */
const opt = (values: [string, string][]) => values.map(([id, name]) => ({ id, name }));

export const riskLevelOptions = opt([
  ["Low", "Low"],
  ["Medium", "Medium"],
  ["High", "High"],
]);

/** User-selectable (operational) review statuses. */
export const talentReviewStatusOptions = opt([
  ["Draft", "Draft"],
  ["InProgress", "In Progress"],
  ["Completed", "Completed"],
]);

/** All review statuses incl. the workflow-owned ones (set by the approval engine, never picked by hand). */
export const talentReviewStatusLabels = opt([
  ["Draft", "Draft"],
  ["InProgress", "In Progress"],
  ["Completed", "Completed"],
  ["PendingApproval", "Pending Approval"],
  ["Rejected", "Rejected"],
]);

export const talentReviewStatusLabel = (id?: string) =>
  talentReviewStatusLabels.find((o) => o.id === id)?.name ?? id ?? "";

/** Critical-position approval states (workflow-owned — never picked by hand). */
export const criticalPositionStatusLabels = opt([
  ["Active", "Active"],
  ["PendingApproval", "Pending Approval"],
  ["Rejected", "Rejected"],
]);

export const criticalPositionStatusLabel = (id?: string) =>
  criticalPositionStatusLabels.find((o) => o.id === id)?.name ?? id ?? "";

/** 9-box band (1 = Low … 3 = High) for both performance and potential axes. */
export const bandOptions = [
  { id: "1", name: "Low" },
  { id: "2", name: "Medium" },
  { id: "3", name: "High" },
];

export const readinessLevelOptions = opt([
  ["ReadyNow", "Ready now"],
  ["Ready1To2Years", "Ready in 1–2 years"],
  ["Ready3PlusYears", "Ready in 3+ years"],
  ["NotReady", "Not ready"],
]);

export const successionHorizonOptions = opt([
  ["ShortTerm", "Short term"],
  ["MediumTerm", "Medium term"],
  ["LongTerm", "Long term"],
]);

/** User-selectable (operational) plan statuses. */
export const successionPlanStatusOptions = opt([
  ["Active", "Active"],
  ["OnHold", "On hold"],
  ["Closed", "Closed"],
]);

/** All plan statuses incl. the workflow-owned ones (set by the approval engine, never picked by hand). */
export const successionPlanStatusLabels = opt([
  ["Active", "Active"],
  ["OnHold", "On hold"],
  ["Closed", "Closed"],
  ["PendingApproval", "Pending Approval"],
  ["Rejected", "Rejected"],
]);

export const successionPlanStatusLabel = (id?: string) =>
  successionPlanStatusLabels.find((o) => o.id === id)?.name ?? id ?? "";

export const successionActionTypeOptions = opt([
  ["Mentorship", "Mentorship"],
  ["Training", "Training"],
  ["JobRotation", "Job rotation"],
  ["Coaching", "Coaching"],
  ["Other", "Other"],
]);

export const successionActionStatusOptions = opt([
  ["Planned", "Planned"],
  ["InProgress", "In Progress"],
  ["Completed", "Completed"],
  ["Cancelled", "Cancelled"],
]);

export const knowledgeTransferStatusOptions = opt([
  ["NotStarted", "Not started"],
  ["InProgress", "In Progress"],
  ["Completed", "Completed"],
]);

/** Human-readable band label (1–3). */
export const bandLabel = (band: number) => bandOptions.find((b) => b.id === String(band))?.name ?? String(band);

// ===== Career Development §3.7.B — Career Path (values = backend enum names) =====

export const careerStepProgressStatusOptions = opt([
  ["NotStarted", "Not started"],
  ["InProgress", "In Progress"],
  ["Completed", "Completed"],
]);

export const employeeCareerPathStatusOptions = opt([
  ["Active", "Active"],
  ["Completed", "Completed"],
  ["OnHold", "On hold"],
]);

export const mentorshipContextOptions = opt([
  ["General", "General"],
  ["CareerPath", "Career path"],
  ["Succession", "Succession"],
]);

export const mentorshipStatusOptions = opt([
  ["Active", "Active"],
  ["Completed", "Completed"],
  ["Cancelled", "Cancelled"],
]);

export const careerPathChangeStatusOptions = opt([
  ["Draft", "Draft"],
  ["Submitted", "Submitted"],
  ["Approved", "Approved"],
  ["Rejected", "Rejected"],
]);
