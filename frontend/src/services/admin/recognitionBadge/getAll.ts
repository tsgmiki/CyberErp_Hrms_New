import type { RecognitionBadgeModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<RecognitionBadgeModel>("RecognitionBadge");
