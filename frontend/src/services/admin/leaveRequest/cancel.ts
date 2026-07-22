import { api } from "@/utils/apiClient";

export default async function cancelLeaveRequest(id: string, reason?: string) {
  return api.post("LeaveRequest/cancel", { id, reason });
}
