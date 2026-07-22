import type AbstractModel from "../AbstractModel";

/** Employee Engagement §3.9.1 (HC203–HC209). */

/** A suggestion / idea to management (HC203); anonymous per HC207. */
export interface SuggestionModel extends AbstractModel {
  title?: string;
  body?: string;
  isAnonymous?: boolean;
  employeeId?: string;
  /** "Anonymous" on anonymous rows. */
  employeeName?: string;
  /** New | UnderReview | Actioned | Closed. */
  status?: string;
  managementResponse?: string;
  submittedOn?: string;
  respondedOn?: string;
}

/** One progress note on a grievance's trail (HC205). */
export interface GrievanceNoteModel {
  id?: string;
  authorEmployeeId?: string;
  authorName?: string;
  note?: string;
  notedAt?: string;
}

/** An escalated issue / grievance (HC205). */
export interface GrievanceModel extends AbstractModel {
  employeeId?: string;
  employeeName?: string;
  category?: string;
  subject?: string;
  details?: string;
  /** Low | Medium | High | Critical. */
  severity?: string;
  isConfidential?: boolean;
  /** Submitted | UnderReview | Resolved | Closed. */
  status?: string;
  assignedToEmployeeId?: string;
  assignedToName?: string;
  resolution?: string;
  submittedOn?: string;
  resolvedOn?: string;
  notes?: GrievanceNoteModel[];
}

/** An organizational announcement (HC206), optionally targeted. */
export interface AnnouncementModel extends AbstractModel {
  title?: string;
  body?: string;
  /** All | Branch | Unit. */
  audience?: string;
  branchId?: string;
  branchName?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  publishFrom?: string;
  publishUntil?: string;
  isPinned?: boolean;
  isActive?: boolean;
}

/** One survey question (survey-JSON shape, HC204). */
export interface SurveyQuestionModel {
  key?: string;
  text?: string;
  /** Rating | Choice | Text. */
  type?: string;
  options?: string[];
  required?: boolean;
}

/** A survey / questionnaire / quick poll (HC204). */
export interface SurveyModel extends AbstractModel {
  title?: string;
  description?: string;
  isPoll?: boolean;
  isAnonymous?: boolean;
  /** Draft | Open | Closed. */
  status?: string;
  opensOn?: string;
  closesOn?: string;
  questionCount?: number;
  responseCount?: number;
  hasResponded?: boolean;
  questions?: SurveyQuestionModel[];
}

/** Aggregated results of one question (HC204). */
export interface SurveyQuestionResultModel {
  key?: string;
  text?: string;
  type?: string;
  answered?: number;
  average?: number;
  /** Rating value / option → count. */
  counts?: Record<string, number>;
  textAnswers?: string[];
}

export interface SurveyResultsModel {
  surveyId?: string;
  title?: string;
  isAnonymous?: boolean;
  status?: string;
  responseCount: number;
  eligibleCount: number;
  completionRatePercent: number;
  questions: SurveyQuestionResultModel[];
}
