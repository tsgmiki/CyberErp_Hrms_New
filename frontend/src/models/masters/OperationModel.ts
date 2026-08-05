import type AbstractModel from "../AbstractModel";

export default interface OperationModel extends AbstractModel {
  name?: string;
  moduleId: string;
  module?: string;
  /** Owning subsystem (via the module) — the System screens filter/group on it. */
  subsystemId?: string;
  link?: string;
  filter?: string;
  icon?: string;
  sortOrder?: number;
  canView?: boolean;
  canAdd?: boolean;
  canEdit?: boolean;
  canDelete?: boolean;
  canApprove?: boolean;
  subSystem?: string;
}
