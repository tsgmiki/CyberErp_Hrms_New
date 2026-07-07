import { LeaveRequestSchema } from "@/components/util/validation";
import { createSaveService } from "@/template/createSaveService";

// Submit only (POST); leave requests are never edited, only cancelled.
export default createSaveService("LeaveRequest", LeaveRequestSchema);
