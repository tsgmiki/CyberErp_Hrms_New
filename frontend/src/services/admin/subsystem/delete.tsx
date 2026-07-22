import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export default async function deleteSubsystem(id: string) {
  const response = await fetch(`${API_BASE_URL}/Subsystem/${id}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!response.ok) {
    const text = await response.text();
    const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
    throw new Error(errorMessageParser(parsed.errors || parsed) || "Delete failed");
  }

  return true;
}
