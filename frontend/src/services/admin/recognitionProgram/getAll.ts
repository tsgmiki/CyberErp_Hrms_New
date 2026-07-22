import type { RecognitionProgramModel } from "@/models";
import { createPagedQuery } from "@/template/createPagedQuery";

export default createPagedQuery<RecognitionProgramModel>("RecognitionProgram");
