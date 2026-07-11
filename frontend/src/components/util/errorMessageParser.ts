/**
 * Flattens API error payloads into a human-readable PLAIN-TEXT message.
 * Every consumer renders the result as text (status lines, toasts, modal errors) — never as
 * HTML — so no markup and no "1 …<br/>" artifacts. A single error is returned verbatim;
 * multiple errors join as numbered lines.
 */
function joinMessages(messages: string[]): string {
  const clean = messages.map((m) => String(m).trim()).filter(Boolean);
  if (clean.length === 0) return "";
  if (clean.length === 1) return clean[0];
  return clean.map((m, i) => `${i + 1}. ${m}`).join("\n");
}

/** Collects the messages out of an ASP.NET Core `errors` dictionary ({ field: [msg] | msg }). */
function flattenErrorsObject(errors: Record<string, unknown>): string[] {
  const messages: string[] = [];
  for (const key of Object.keys(errors)) {
    const value = errors[key];
    if (Array.isArray(value)) {
      for (const msg of value) messages.push(String(msg));
    } else if (value != null) {
      messages.push(String(value));
    }
  }
  return messages;
}

export default function errorMessageParser(error?: any): string {
  if (typeof error === "string" && error !== "") return error;
  if (!error) return "";

  // Axios-style wrapper: error.response.data.{errors|detail|title}
  if (typeof error.response !== "undefined") {
    const data = error?.response?.data;
    if (data?.errors && typeof data.errors === "object")
      return joinMessages(flattenErrorsObject(data.errors));
    if (data?.detail) return String(data.detail);
    if (data?.title) return String(data.title);
    return "";
  }

  if (typeof error.message !== "undefined") return String(error.message);

  // ASP.NET Core validation payload passed directly ({ errors: { field: [msg] } })
  if (error.errors && typeof error.errors === "object")
    return joinMessages(flattenErrorsObject(error.errors));

  if (error.title) return String(error.title);

  // A bare errors dictionary ({ field: [msg] | msg })
  if (typeof error === "object") return joinMessages(flattenErrorsObject(error));

  return String(error);
}
