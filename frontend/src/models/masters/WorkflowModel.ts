import type AbstractModel from "../AbstractModel";

/**
 * Model for Workflow configuration
 * Defines workflow steps and approval statuses for different voucher types
 */
export default interface WorkflowModel extends AbstractModel {
  /** Type of voucher this workflow applies to */
  voucherType?: string;
  /** Step number in the workflow */
  step?: number;
  /** ID of the status */
  statusId?: string;
  /** Name of the status */
  status?: string;
  /** Criteria for this workflow step */
  criteria?: string;
}
