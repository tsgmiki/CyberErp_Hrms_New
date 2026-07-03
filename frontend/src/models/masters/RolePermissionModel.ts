import type AbstractModel from '../AbstractModel'

export default interface RolePermissionModel extends AbstractModel {
  roleId?: string;
  role?: string;
  operationId?: string;
  operation?: string;
  module?: string;
  canView?: boolean;
  canAdd?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
  canApprove?: boolean;
  details: RolePermissionModel[];
}
