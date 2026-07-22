import type { AuditLogModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AuditLogModel>("AuditLog");
