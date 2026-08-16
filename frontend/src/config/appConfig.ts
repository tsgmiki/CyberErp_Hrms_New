/**
 * Central env-backed configuration for white-label / multi-project deployments.
 * Import from here instead of `import.meta.env` scattered across the app.
 */
export const appConfig = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL as string,
  appName: (import.meta.env.VITE_APP_NAME as string | undefined) ?? "Cyber HRMS",
  defaultLocale: (import.meta.env.VITE_DEFAULT_LOCALE as string | undefined) ?? "en",
} as const;

/**
 * Where each subsystem's WEB APP lives — the landing page deep-links to another subsystem's
 * application through this.
 *
 *   VITE_SUBSYSTEM_APPS={"HOME":"http://localhost:5175","HRMS":"http://localhost:5174"}
 *
 * ⚠️ This replaces `Core.Subsystem.Url`, dropped on 2026-08-16 so the table matches SRMS exactly.
 * The column was the wrong home for it: those rows are shared across every environment while the
 * address differs BY environment, which a single shared row cannot express. Keys are subsystem
 * CODES, matched case-insensitively. The Home portal reads the SAME variable — keep them in step.
 */
function parseSubsystemApps(raw: unknown): Record<string, string> {
  if (typeof raw !== "string" || !raw.trim()) return {};
  try {
    const parsed = JSON.parse(raw);
    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
      console.error("VITE_SUBSYSTEM_APPS must be a JSON object of {code: appUrl} — ignoring it.");
      return {};
    }
    return Object.fromEntries(
      Object.entries(parsed as Record<string, unknown>)
        .filter(([code, url]) => code.trim() && typeof url === "string" && url.trim())
        .map(([code, url]) => [code.trim(), String(url).trim().replace(/\/+$/, "")]),
    );
  } catch {
    // A bad value must not break the landing page — fall back to local scoping.
    console.error("VITE_SUBSYSTEM_APPS is not valid JSON — ignoring it.");
    return {};
  }
}

const SUBSYSTEM_APPS = parseSubsystemApps(import.meta.env.VITE_SUBSYSTEM_APPS);

/** A subsystem application's absolute URL by its code, or undefined when not configured. */
export function appUrlFor(code: string | undefined | null): string | undefined {
  if (!code?.trim()) return undefined;
  const wanted = code.trim().toLowerCase();
  return Object.entries(SUBSYSTEM_APPS).find(([key]) => key.toLowerCase() === wanted)?.[1];
}
