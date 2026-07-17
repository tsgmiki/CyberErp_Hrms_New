import type { SuccessionCandidateModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<SuccessionCandidateModel>("SuccessionCandidate");
