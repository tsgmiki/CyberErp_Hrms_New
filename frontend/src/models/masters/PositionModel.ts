import type AbstractModel from "../AbstractModel";

export default interface PositionModel extends AbstractModel {
  code?: string;
  positionClassId?: string;
  positionClassTitle?: string;
  organizationUnitId?: string;
  organizationUnitName?: string;
  branchId?: string;
  branchName?: string;
  isVacant?: boolean;
}
