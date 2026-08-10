import { ReviewCycleSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

export default createSaveService("ReviewCycle", ReviewCycleSchema, {
  // EVERY bool on SaveReviewCycleDto must be listed — an omitted one is sent as the STRING "false",
  // which fails JSON binding and nulls the whole DTO ("The dto field is required").
  booleanFields: [
    "enableSelfAssessment", "enablePeerAssessment", "enableCalibration",
    "enableSecondLevelReview", "enableHrSignOff",
  ],
  integerFields: ["probationDurationMonths"],
});
