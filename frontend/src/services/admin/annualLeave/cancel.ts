import { api } from "@/utils/apiClient";

export default async function cancelAnnualLeave(id: string) {
  return api.post("AnnualLeave/cancel", { id });
}
