import type { AchievementModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<AchievementModel>("Achievement");
