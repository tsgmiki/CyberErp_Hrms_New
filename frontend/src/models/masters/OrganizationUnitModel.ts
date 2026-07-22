import type AbstractModel from "../AbstractModel";

export default interface OrganizationUnitModel extends AbstractModel {
  code?: string;
  name?: string;
  unitType?: string;
  branchId?: string;
  branchName?: string;
  parentId?: string;
  parentName?: string;
  workLocationId?: string;
  workLocationName?: string;
  allocatedHeadcount?: number;
  description?: string;
  isActive?: boolean;
  hasChildren?: boolean;
  children?: OrganizationUnitModel[];
}

/** Nested node returned by GET OrganizationUnit/tree (org chart). */
export interface OrgUnitTreeNode {
  id: string;
  code: string;
  name: string;
  unitType: string;
  allocatedHeadcount?: number;
  children: OrgUnitTreeNode[];
}
