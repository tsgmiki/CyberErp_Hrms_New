import type { OrgUnitTreeNode } from "@/models";
import { api } from "@/utils/apiClient";

/** GET OrganizationUnit/tree — full nested hierarchy for the org chart. */
export default async function getOrganizationTree(): Promise<OrgUnitTreeNode[]> {
  return api.get<OrgUnitTreeNode[]>("OrganizationUnit/tree");
}
