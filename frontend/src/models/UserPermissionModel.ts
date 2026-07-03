import type AbstractModel from "./AbstractModel";

export default interface UserPermissionModel extends AbstractModel {
  userId?: string;
  user?: string;
  operationId?: string;
  operation?: string;
  module?: string;
  link?: string;
  canView?: boolean;
  canAdd?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
  canApprove?: boolean;
  details: UserPermissionModel[];
}
