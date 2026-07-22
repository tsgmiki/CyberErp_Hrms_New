import type { TalentAssessmentModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<TalentAssessmentModel>("TalentAssessment");
