import { ReviewCycleSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("ReviewCycle", ReviewCycleSchema, {
  booleanFields: ["enableSelfAssessment", "enablePeerAssessment", "enableCalibration"],
  integerFields: ["probationDurationMonths"],
});
