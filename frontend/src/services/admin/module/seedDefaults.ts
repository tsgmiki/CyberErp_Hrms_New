import errorMessageParser from "@/components/util/errorMessageParser";
import isValidJson from "@/components/util/validateJson";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Seeds the default HRMS menu (subsystem + modules + operations) for the current tenant. */
export default async function seedDefaultMenu(): Promise<{ ok: boolean; message: string }> {
  const response = await fetch(`${API_BASE_URL}/Module/seed-defaults`, {
    method: "POST",
    credentials: "include",
  });
  const text = await response.text();
  const parsed = isValidJson(text) ? JSON.parse(text) : { message: text };
  if (!response.ok) {
    return { ok: false, message: errorMessageParser(parsed.errors || parsed) || "Seeding failed" };
  }
  return { ok: true, message: parsed?.message ?? "Menu seeded" };
}
