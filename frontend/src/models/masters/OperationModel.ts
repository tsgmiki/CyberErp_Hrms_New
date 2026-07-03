import type AbstractModel from "../AbstractModel";

export default interface OperationModel extends AbstractModel {
  name?: string;
  moduleId: string;
  module?: string;
  link?: string;
  canView?: boolean;
  canAdd?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
  canApprove?: boolean;
  subSystem?: string;
}
