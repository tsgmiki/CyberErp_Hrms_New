import type AbstractModel from "../AbstractModel";

/** Hiring-need assessment (HC077–HC083). */
export interface HiringRequestModel extends AbstractModel {
  requestNumber?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  numberOfPositions?: number;
  employmentType?: string;
  justification?: string;
  jobRequirements?: string;
  expectedStartDate?: string;
  timelineRemarks?: string;
  estimatedBudget?: number;
  workforcePlanId?: string;
  workforcePlanName?: string;
  status?: string; // Draft | Submitted | Approved | Rejected | Closed
  submittedAt?: string;
  approvedAt?: string;
  awaitingWorkflow?: boolean;
  vacantSeats?: number;
  requisitionedPositions?: number;
}

/** Per-unit recruitment budget/headcount monitor row (HC083). */
export interface RecruitmentBudgetRowModel {
  organizationUnitId: string;
  organizationUnitName?: string;
  approvedRequests: number;
  requestedPositions: number;
  estimatedBudget: number;
  openRequisitions: number;
}

/** One evaluator assigned to a criterion — internal employee or named external. */
export interface CriterionEvaluatorModel {
  /** Employee | ExternalPerson | Organization */
  evaluatorType: string;
  employeeId?: string;
  /** External evaluator's name, or the resolved employee name on reads. */
  name?: string;
}

export interface ScreeningCriterionModel {
  id?: string;
  name: string;
  isMandatory: boolean;
  /** Percentage of the final ranking score — all criteria must total exactly 100. */
  weight: number;
  /** The evaluators assigned to score this criterion (any number, mixed kinds). */
  evaluators?: CriterionEvaluatorModel[];
  /** Recruitment level (e.g. Screening, Interview) — empty = all steps. */
  appliesAtStage?: string;
}

/** One evaluator's score of one criterion (score sheet row). */
export interface CriterionScoreModel {
  criterionId: string;
  criterionName?: string;
  isMandatory: boolean;
  weight: number;
  /** Display list of the criterion's assigned evaluators ("A, B, …"). */
  evaluatorName?: string;
  appliesAtStage?: string;
  score?: number;
  remarks?: string;
  scoredBy?: string;
  scoredAt?: string;
}

/** One row of a vacancy's auto-calculated candidate ranking. */
export interface ApplicationRankingRowModel {
  applicationId: string;
  candidateId: string;
  candidateNumber?: string;
  candidateName?: string;
  stage: string;
  /** Application date — the documented tie-break basis (earliest first). */
  appliedAt?: string;
  totalScore?: number;
  scoredCriteria: number;
  totalCriteria: number;
  failsMandatory: boolean;
  /** Standard-competition rank (ties SHARE a rank: 1,1,1,4…). */
  rank?: number;
  /** True when another in-contention candidate has the exact same weighted total. */
  tied?: boolean;
  /** Eligible | Waitlisted | Hired | OfferRejected | OutOfContention | FailsMandatory | NotScored */
  hireEligibility?: string;
  latestOfferStatus?: string;
  breakdown: CriterionScoreModel[];
}

/** One hire-ready (or waitlisted) applicant on the "Hire Employee" screen. */
export interface HireQueueRowModel {
  requisitionId: string;
  requisitionNumber?: string;
  requisitionTitle?: string;
  numberOfPositions: number;
  hiredCount: number;
  applicationId: string;
  candidateId: string;
  candidateNumber?: string;
  candidateName?: string;
  stage: string;
  totalScore?: number;
  rank?: number;
  hireEligibility: string;
  latestOfferStatus?: string;
  complianceComplete: boolean;
  missingComplianceDocuments: string[];
  canHire: boolean;
  blockedReason?: string;
}

/** A file attached to a candidate (credentials + mandatory compliance set). */
export interface CandidateDocumentModel {
  id: string;
  documentType: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
}

/** Job requisition (HC084–HC088, HC091, HC095). */
export interface JobRequisitionModel extends AbstractModel {
  requisitionNumber?: string;
  hiringRequestId?: string;
  hiringRequestNumber?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  jobGradeName?: string;
  workLocationId?: string;
  workLocationName?: string;
  numberOfPositions?: number;
  employmentType?: string;
  title?: string;
  description?: string;
  minQualifications?: string;
  minExperienceYears?: number;
  skills?: string;
  salaryScaleId?: string;
  salaryScaleAmount?: number;
  postingChannel?: string; // Internal | External | Both
  postingText?: string;
  openFrom?: string;
  openUntil?: string;
  status?: string; // Draft | PendingApproval | Approved | Posted | Closed | Cancelled | Rejected
  submittedAt?: string;
  approvedAt?: string;
  postedAt?: string;
  closedAt?: string;
  awaitingWorkflow?: boolean;
  applicationCount?: number;
  screeningCriteria?: ScreeningCriterionModel[];
}

/** Centralized applicant record (HC092–HC097). */
export interface CandidateModel extends AbstractModel {
  candidateNumber?: string;
  firstName?: string;
  fatherName?: string;
  grandFatherName?: string;
  fullName?: string;
  email?: string;
  phoneNumber?: string;
  gender?: string;
  source?: string; // External | Internal | JobBoard | SocialMedia | Referral | WalkIn
  internalEmployeeId?: string;
  internalEmployeeName?: string;
  educationSummary?: string;
  experienceSummary?: string;
  skillsSummary?: string;
  yearsOfExperience?: number;
  resumeFileName?: string;
  consentGiven?: boolean;
  consentAt?: string;
  isArchived?: boolean;
  anonymizedAt?: string;
  isInTalentPool?: boolean;
  talentPoolNotes?: string;
  applicationCount?: number;
  /** The shared CorePerson record backing this candidate (hire-conversion anchor). */
  personId?: string;
  /** The employee this candidate became when hired. */
  hiredEmployeeId?: string;
  missingComplianceDocuments?: string[];
  complianceComplete?: boolean;
}

// Candidate education/experience are the SAME person-owned rows as the employee profile, so they use
// the shared EmployeeEducation/ExperienceModel types — see components/common/personBackground.

/** One ranked internal-matching result for a vacancy (HC090). */
export interface CandidateMatchModel {
  candidateId: string;
  candidateNumber: string;
  fullName: string;
  source: string;
  isInTalentPool: boolean;
  yearsOfExperience?: number;
  matchScore: number;
  matchedSkills: string[];
  meetsExperience: boolean;
}

/** One panelist's per-criterion score of an interview round (HC106/HC109). */
export interface InterviewFeedbackModel {
  criterionId?: string;
  criterionName?: string;
  score: number;
  comments?: string;
  submittedAt?: string;
}

export interface InterviewPanelistModel {
  id?: string;
  employeeId?: string;
  panelistName?: string;
  isLead?: boolean;
  attendance?: string;
  feedback?: InterviewFeedbackModel[];
  averageScore?: number;
}

/** One interview round of an application (HC101–HC108). */
export interface InterviewModel {
  id?: string;
  applicationId?: string;
  round?: number;
  scheduledStart?: string;
  scheduledEnd?: string;
  format?: string; // InPerson | Video | Phone | Panel | TechnicalTest
  status?: string; // Scheduled | Completed | Cancelled | NoShow
  location?: string;
  meetingLink?: string;
  notes?: string;
  panelists?: InterviewPanelistModel[];
  averageScore?: number;
}

export interface InterviewCriterionSummaryModel {
  criterionId?: string;
  criterionName: string;
  /** Weight (%) inherited from the requisition criterion (0 for overall entries). */
  weight: number;
  average: number;
  scores: number;
}

/** HC109 — consolidated evaluation of one application across all rounds. */
export interface InterviewConsolidatedModel {
  applicationId: string;
  rounds: number;
  panelistCount: number;
  scoredPanelists: number;
  overallAverage?: number;
  /** Weighted by the requisition criteria weights (inherited, not re-entered). */
  weightedAverage?: number;
  criteria: InterviewCriterionSummaryModel[];
  interviews: InterviewModel[];
}

/** Formal employment offer (HC111–HC114). */
export interface JobOfferModel {
  id?: string;
  offerNumber?: string;
  applicationId?: string;
  hiringManagerEmployeeId?: string;
  hiringManagerName?: string;
  salary?: number;
  salaryScaleId?: string;
  salaryScaleAmount?: number;
  salaryJustification?: string;
  proposedStartDate?: string;
  expiryDate?: string;
  status?: string; // Draft | PendingApproval | Approved | Sent | Accepted | Declined | Withdrawn | Expired
  sentAt?: string;
  respondedAt?: string;
  responseNote?: string;
  letterText?: string;
  hiredEmployeeId?: string;
  awaitingWorkflow?: boolean;
}

/** Vacancy-derived defaults for a new offer (position scale + unit-hierarchy manager). */
export interface OfferDefaultsModel {
  requisitionId: string;
  requisitionTitle?: string;
  organizationUnitId: string;
  unitName?: string;
  positionClassId: string;
  positionTitle?: string;
  salaryScaleId?: string;
  salaryScaleAmount?: number;
  salaryScaleLabel?: string;
  hiringManagerEmployeeId?: string;
  hiringManagerName?: string;
  /** The unit whose manager answered — the vacancy's unit or an ancestor. */
  managerResolvedFromUnit?: string;
}

/** Mass stage move outcome — per-item reporting, never all-or-nothing. */
export interface BulkMoveResultModel {
  moved: number;
  skipped: { applicationId: string; candidateName?: string; reason: string }[];
}

export interface ApplicationStageLogModel {
  stage: string;
  note?: string;
  actedBy?: string;
  actedAt: string;
}

/** One candidate's application to one requisition (HC098–HC099). */
export interface JobApplicationModel extends AbstractModel {
  candidateId?: string;
  candidateNumber?: string;
  candidateName?: string;
  requisitionId?: string;
  requisitionNumber?: string;
  requisitionTitle?: string;
  stage?: string;
  appliedAt?: string;
  screeningScore?: number;
  screeningRemarks?: string;
  /** Total criteria defined on the vacancy. */
  totalCriteriaCount?: number;
  /**
   * Criteria scoreable at the application's CURRENT stage — global criteria always count,
   * level-scoped ones only while the application sits at that level. Drives score-button visibility.
   */
  scoreableCriteriaCount?: number;
  /**
   * Hire/offer eligibility from the vacancy ranking (Eligible | Waitlisted | NotScored |
   * FailsMandatory | OfferRejected | Hired | OutOfContention); undefined when the vacancy has no
   * weighted criteria. The Offer button activates only for Eligible applicants.
   */
  hireEligibility?: string;
  /** 1-based weighted-total rank within the vacancy. */
  rank?: number;
  stageLog?: ApplicationStageLogModel[];
  /** The requisition's criteria merged with this application's scores (score sheet). */
  criterionScores?: CriterionScoreModel[];
}
