import type AbstractModel from "../AbstractModel";

export default interface BranchModel extends AbstractModel {
  code?: string;
  name?: string;
  description?: string;
  address?: string;
  isHeadOffice?: boolean;
  isActive?: boolean;
  parentId?: string;
  parentName?: string;
  hasChildren?: boolean;
  children?: BranchModel[];
}
