const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

/** Creates the built-in starter templates (e.g. the Clearance Certificate) when absent (idempotent). */
export default async function seedDefaultDocumentTemplates(): Promise<{ ok: boolean; message: string }> {
  const res = await fetch(`${API_BASE_URL}/DocumentTemplate/seed-defaults`, {
    method: "POST",
    credentials: "include",
  });
  const text = await res.text();
  let message = "Default templates created";
  try {
    message = JSON.parse(text)?.message ?? message;
  } catch {
    if (text) message = text;
  }
  return { ok: res.ok, message };
}
