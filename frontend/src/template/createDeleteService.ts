import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Factory for the standard delete service (`DELETE /{resource}/{id}`). */
export function createDeleteService(resource: string) {
  const path = resource.replace(/^\//, "").replace(/\/$/, "");
  return async function remove(id: string) {
    try {
      const response = await fetch(`${API_BASE_URL}/${path}/${id}`, {
        method: "DELETE",
        credentials: "include",
      });
      if (response.ok) {
        const text = await response.text();
        return isValidJson(text) ? JSON.parse(text) : { status: "success", message: "Deleted" };
      }
      const text = await response.text();
      const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
      const message =
        typeof parsed.reason !== "undefined" ? parsed.reason : errorMessageParser(parsed.errors || parsed);
      return { status: "error", message };
    } catch (error) {
      return { status: "error", message: errorMessageParser(error) };
    }
  };
}
