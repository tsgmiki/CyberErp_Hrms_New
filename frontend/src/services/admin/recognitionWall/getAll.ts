import type { RecognitionWallItemModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<RecognitionWallItemModel>("RecognitionWall");
