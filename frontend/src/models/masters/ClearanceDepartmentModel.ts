import type AbstractModel from "../AbstractModel";
import type { WorkflowApproverModel } from "./HrWorkflowModel";

/**
 * Admin-configured clearance department for employee offboarding (mirrors the workflow engine's
 * approver pattern). Active departments form the termination clearance checklist; a department
 * with no approvers is open (anyone may clear it), otherwise any single authorized user's
 * approval clears it.
 */
export default interface ClearanceDepartmentModel extends AbstractModel {
  name?: string;
  /** Requirement text shown on the checklist item (what must be returned/settled). */
  description?: string;
  sortOrder?: number;
  isActive?: boolean;
  approvers?: WorkflowApproverModel[];
}
