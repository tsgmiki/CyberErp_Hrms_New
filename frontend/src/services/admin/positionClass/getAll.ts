import type { PositionClassModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<PositionClassModel>("PositionClass");
