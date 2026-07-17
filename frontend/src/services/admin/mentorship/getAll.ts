import type { MentorshipModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";
export default createPagedQuery<MentorshipModel>("Mentorship");
