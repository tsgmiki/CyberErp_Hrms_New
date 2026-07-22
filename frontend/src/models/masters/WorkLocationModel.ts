import type AbstractModel from "../AbstractModel";

export default interface WorkLocationModel extends AbstractModel {
  code?: string;
  name?: string;
  locationType?: string;
  parentId?: string;
  parentName?: string;
  address?: string;
  description?: string;
  isActive?: boolean;
  hasChildren?: boolean;
  children?: WorkLocationModel[];
}
