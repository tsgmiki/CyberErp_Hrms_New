import type AbstractModel from "../AbstractModel";

/**
 * One authorized approver of a step: a specific user, any user holding a role, or a DYNAMIC
 * approver resolved from the org structure per request — "ImmediateManager" (the requester's unit
 * manager, climbing parent units) or "UnitManager" (the manager of a configured unit; approverId
 * holds the org-unit id). ImmediateManager carries the empty guid as approverId.
 */
export interface WorkflowApproverModel {
  approverType: "User" | "Role" | "ImmediateManager" | "UnitManager";
  approverId: string;
  displayName?: string;
}

/** approverId placeholder for ImmediateManager approvers (no target — resolved per request). */
export const EMPTY_APPROVER_ID = "00000000-0000-0000-0000-000000000000";

/** One ordered approval step of a workflow definition. */
export interface WorkflowStepModel {
  stepOrder: number;
  name: string;
  approverRole?: string | null;
  /** Empty = any authenticated user may act (open step). */
  approvers?: WorkflowApproverModel[];
}

/** Admin-configured approval chain for one HR process (entityType key). */
export interface WorkflowDefinitionModel extends AbstractModel {
  name?: string;
  entityType?: string; // e.g. EmployeeMovement.Transfer
  description?: string;
  isActive?: boolean;
  steps?: WorkflowStepModel[];
}

/** A running / finished approval for one business record. */
export interface WorkflowInstanceModel extends AbstractModel {
  definitionName?: string;
  entityType?: string;
  entityId?: string;
  employeeId?: string;
  summary?: string;
  status?: string; // Running | Approved | Rejected | Cancelled
  currentStepOrder?: number;
  currentStepName?: string;
  totalSteps?: number;
  requestedBy?: string;
  requestedAt?: string;
  completedAt?: string;
  /** Whether the current user may act on the current step. */
  canDecide?: boolean;
  /** Display names of the current step's authorized approvers (empty = anyone). */
  currentStepApprovers?: string[];
}

/** One approval awaiting the current user's decision (Dashboard "Approvals" inbox). */
export interface MyApprovalItemModel {
  instanceId: string;
  summary: string;
  entityType: string;
  currentStepOrder: number;
  currentStepName: string;
  totalSteps: number;
  requestedBy?: string;
  requestedAt?: string;
}

/** The current user's approval inbox + whether they are an assigned approver at all. */
export interface MyApprovalsModel {
  isApprover: boolean;
  items: MyApprovalItemModel[];
}

/** One decision on a workflow instance. */
export interface WorkflowActionModel {
  stepOrder: number;
  stepName: string;
  action: string; // Submitted | Approved | Rejected | Cancelled
  comment?: string;
  actedBy?: string;
  actedAt?: string;
}

export interface WorkflowStatsModel {
  running: number;
  approved: number;
  rejected: number;
}
