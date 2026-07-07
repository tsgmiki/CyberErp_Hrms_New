import type AbstractModel from "../AbstractModel";

/** One authorized approver of a step: a specific user, or any user holding a role. */
export interface WorkflowApproverModel {
  approverType: "User" | "Role";
  approverId: string;
  displayName?: string;
}

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
