import type AbstractModel from "../AbstractModel";

export default interface AuditLogModel extends AbstractModel {
  entityType?: string;
  entityId?: string;
  entityName?: string;
  action?: string;
  changes?: string;
  performedByUserId?: string;
  performedBy?: string;
  branchId?: string;
  timestamp?: string;
}
