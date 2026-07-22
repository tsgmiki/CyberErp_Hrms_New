import type { HolidayModel } from "@/models";
import { createEntityGetById } from "@/template/createEntityGetById";

export default createEntityGetById<HolidayModel>("Holiday");
