// Performance Management (HC118–HC147) — Phase A option constants.

export const ratingScoreTypeOptions = ["Numeric", "Percentage"].map((v) => ({ id: v, name: v }));

export const reviewPeriodTypeOptions = [
  { id: "Annual", name: "Annual" },
  { id: "BiAnnual", name: "Bi-Annual" },
  { id: "Quarterly", name: "Quarterly" },
  { id: "Probation", name: "Probation" },
  { id: "Custom", name: "Custom" },
];

export const reviewPeriodTypeLabel = (id?: string) =>
  reviewPeriodTypeOptions.find((o) => o.id === id)?.name ?? id ?? "";

export const reviewCycleStatusOptions = ["Draft", "Active", "Closed"].map((v) => ({ id: v, name: v }));

export const objectiveStatusOptions = ["Draft", "Active", "Closed"].map((v) => ({ id: v, name: v }));

export const goalStatusOptions = ["Draft", "Active", "Completed", "Cancelled"].map((v) => ({ id: v, name: v }));

export const appraisalStageOptions = [
  { id: "SelfAssessment", name: "Self-Assessment" },
  { id: "ManagerReview", name: "Manager Review" },
  { id: "SecondLevelReview", name: "2nd-Level Review" },
  { id: "EmployeeAcknowledgment", name: "Employee Sign-off" },
  { id: "HrSignOff", name: "HR Sign-off" },
  { id: "Completed", name: "Completed" },
];

export const appraisalStageLabel = (id?: string) =>
  appraisalStageOptions.find((o) => o.id === id)?.name ?? id ?? "";

export const calibrationStatusOptions = ["Draft", "Finalized"].map((v) => ({ id: v, name: v }));

// Phase D1 — development / improvement plans
export const developmentPlanStatusOptions = ["Draft", "Active", "Completed", "Cancelled"].map((v) => ({ id: v, name: v }));

export const developmentActionStatusOptions = [
  { id: "Planned", name: "Planned" },
  { id: "InProgress", name: "In Progress" },
  { id: "Completed", name: "Completed" },
];

export const learningInterventionOptions = ["Course", "Mentoring", "On-the-job", "Certification", "Workshop", "Reading"].map((v) => ({ id: v, name: v }));

export const pipStatusOptions = [
  { id: "Draft", name: "Draft" },
  { id: "Active", name: "Active" },
  { id: "UnderReview", name: "Under Review" },
  { id: "Completed", name: "Completed" },
];

export const pipObjectiveStatusOptions = [
  { id: "NotStarted", name: "Not Started" },
  { id: "InProgress", name: "In Progress" },
  { id: "Met", name: "Met" },
  { id: "NotMet", name: "Not Met" },
];

export const pipOutcomeOptions = [
  { id: "Successful", name: "Successful" },
  { id: "Unsuccessful", name: "Unsuccessful" },
  { id: "Extended", name: "Extended" },
];

// Phase D2 — achievements & recognition
export const achievementCategoryOptions = ["Milestone", "Award", "Project", "Certification", "Other"].map((v) => ({ id: v, name: v }));

// Phase D3 — appeals
export const appealStatusOptions = [
  { id: "Open", name: "Open" },
  { id: "UnderReview", name: "Under Review" },
  { id: "Resolved", name: "Resolved" },
  { id: "Rejected", name: "Rejected" },
];
