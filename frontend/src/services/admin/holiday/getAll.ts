import type { HolidayModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<HolidayModel>("Holiday");
