import type AbstractModel from "../AbstractModel";

export default interface EmployeeModel extends AbstractModel {
  personId?: string;
  employeeNumber?: string;
  // Person (Core.CorePerson) — Ethiopian naming with Amharic variants
  firstName?: string;
  firstNameA?: string;
  fatherName?: string;
  fatherNameA?: string;
  grandFatherName?: string;
  grandFatherNameA?: string;
  fullName?: string;
  gender?: string;
  maritalStatus?: string;
  nationalityId?: string;
  phoneNumber?: string;
  locationName?: string;
  // Employment record
  employmentStatus?: string;
  employmentNature?: string;
  contractPeriod?: number;
  isProbation?: boolean | string;
  probationEndDate?: string;
  isTerminated?: boolean;
  dateOfBirth?: string;
  placeOfBirth?: string;
  spouseName?: string;
  email?: string;
  photoUrl?: string;
  nationalId?: string;
  tin?: string;
  pensionNumber?: string;
  hireDate?: string;
  jobGradeId?: string;
  jobGradeName?: string;
  salaryScaleId?: string;
  salaryScaleStep?: string;
  salaryScaleAmount?: number;
  salary?: number;

  positionId?: string;
  positionCode?: string;
  positionClassTitle?: string;
  /** Derived from the position's organization unit (read-only). */
  organizationUnitId?: string;
  organizationUnitName?: string;
  branchId?: string;
  branchName?: string;

  /** Dynamic field values keyed by field definition name (HC021). */
  customFields?: Record<string, string | null>;
}

export interface EmployeeEducationModel extends AbstractModel {
  employeeId?: string;
  educationLevel?: string;
  institution?: string;
  fieldOfStudy?: string;
  qualification?: string;
  graduationYear?: number;
  remark?: string;
  documentCount?: number;
}

export interface EmployeeExperienceModel extends AbstractModel {
  employeeId?: string;
  organization?: string;
  jobTitle?: string;
  startDate?: string;
  endDate?: string;
  responsibilities?: string;
  documentCount?: number;
}

/** A file attached to an education/experience record (HC017/HC018). */
export interface EmployeeDocumentModel {
  id: string;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: string;
}

/** Personnel action (SAP-style): transfer / promotion / demotion with from→to snapshot. */
export interface EmployeeMovementModel extends AbstractModel {
  employeeId?: string;
  movementType?: string; // Transfer | Promotion | Demotion
  status?: string; // Pending | Completed | Cancelled
  effectiveDate?: string;
  fromPositionId?: string;
  fromPositionName?: string;
  fromJobGradeId?: string;
  fromJobGradeName?: string;
  fromSalary?: number;
  fromBranchName?: string;
  toPositionId?: string;
  toPositionName?: string;
  toJobGradeId?: string;
  toJobGradeName?: string;
  toSalary?: number;
  toBranchName?: string;
  reason?: string;
  remark?: string;
  executedAt?: string;
}

/** One departmental clearance item of a termination case. */
export interface TerminationClearanceModel {
  id: string;
  department: string;
  description: string;
  status: string; // Pending | Cleared | Blocked
  note?: string;
  clearedBy?: string;
  clearedAt?: string;
  /** Whether the current user is authorized to decide this department's clearance. */
  canDecide?: boolean;
  /** Configured approver display names (empty = open — anyone may clear). */
  approverNames?: string[];
}

/** One outstanding clearance item assigned to the current user (Dashboard queue). */
export interface MyClearanceItemModel {
  clearanceId: string;
  terminationId: string;
  employeeId: string;
  employeeName: string;
  employeeNumber: string;
  department: string;
  description: string;
  status: string; // Pending | Blocked
  note?: string;
  lastWorkingDate?: string;
}

/** The current user's clearance queue + whether they are an assigned approver at all. */
export interface MyClearancesModel {
  isApprover: boolean;
  items: MyClearanceItemModel[];
}

/** One row of the Termination List: a terminated employee + their latest case. */
export interface TerminatedEmployeeModel {
  id?: string; // list template expects `id` — mapped from employeeId
  employeeId: string;
  employeeNumber: string;
  fullName: string;
  photoUrl?: string;
  email?: string;
  phoneNumber?: string;
  hireDate?: string;
  terminationId?: string;
  terminationType?: string;
  noticeDate?: string;
  lastWorkingDate?: string;
  settledAt?: string;
  reason?: string;
}

/** Termination & clearance case (offboarding). */
export interface EmployeeTerminationModel extends AbstractModel {
  employeeId?: string;
  terminationType?: string; // Voluntary | Involuntary
  status?: string; // Initiated | ClearanceInProgress | Settled | Cancelled
  noticeDate?: string;
  lastWorkingDate?: string;
  reason?: string;
  remarks?: string;
  settledAt?: string;
  awaitingWorkflow?: boolean;
  clearances?: TerminationClearanceModel[];
}

/** Disciplinary case record. */
export interface DisciplinaryMeasureModel extends AbstractModel {
  employeeId?: string;
  violationDate?: string;
  violationType?: string;
  description?: string;
  measureType?: string;
  status?: string; // Open | UnderReview | Resolved | Cancelled
  effectiveDate?: string;
  resolution?: string;
}

export interface EmployeeDependentModel extends AbstractModel {
  employeeId?: string;
  fullName?: string;
  relationship?: string;
  dateOfBirth?: string;
  phoneNumber?: string;
  address?: string;
  isDependent?: boolean;
  relatedEmployeeId?: string;
  relatedEmployeeName?: string;
  remark?: string;
}
