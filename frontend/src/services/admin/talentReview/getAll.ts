import type { TalentReviewModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<TalentReviewModel>("TalentReview");
