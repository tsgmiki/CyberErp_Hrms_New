import type { OrganizationUnitModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<OrganizationUnitModel>("OrganizationUnit");
