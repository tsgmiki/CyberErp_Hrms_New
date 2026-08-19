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
 *   VITE_SUBSYSTEM_APPS={"SSMS":"http://localhost:5175","HRMS":"http://localhost:5174"}
 *
 * ⚠️ This replaces `Core.Subsystem.Url`, dropped on 2026-08-16 so the table matches SRMS exactly.
 * The column was the wrong home for it: those rows are shared across every environment while the
 * address differs BY environment, which a single shared row cannot express. Keys are subsystem ABBREVIATIONS, matched case-insensitively. The Home portal reads the SAME variable — keep them in step.
 */
function parseSubsystemApps(raw: unknown): Record<string, string> {
  if (typeof raw !== "string" || !raw.trim()) return {};
  try {
    const parsed = JSON.parse(raw);
    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
      console.error("VITE_SUBSYSTEM_APPS must be a JSON object of {abbreviation: appUrl} — ignoring it.");
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

/**
 * The Home portal's own subsystem, by abbreviation. HRMS excludes its card from the launcher —
 * the flow is one-way, Home into HRMS and never back.
 *
 * ⚠️ Defined ONCE. The Home portal learned this the hard way: the same identity was copy-pasted
 * into several files and they drifted, one looking for '003' while the catalogue said 'HOME',
 * which silently mis-routes links instead of failing. Import it; never re-declare it.
 */
export const HOME_SUBSYSTEM_ABBREVIATION = "SSMS";

/** A subsystem application's absolute URL by its abbreviation, or undefined when not configured. */
export function appUrlFor(abbreviation: string | undefined | null): string | undefined {
  if (!abbreviation?.trim()) return undefined;
  const wanted = abbreviation.trim().toLowerCase();
  return Object.entries(SUBSYSTEM_APPS).find(([key]) => key.toLowerCase() === wanted)?.[1];
}

/**
 * THIS application's own subsystem, by abbreviation — the sidebar's default scope and the identity
 * HRMS renders menus for.
 *
 * ⚠️ Scope on the ABBREVIATION, never on the subsystem's display NAME. The name is a label an
 * administrator can rename, and on 2026-08-19 one did: "HRMS" became
 * "Human Resource Management System". The sidebar compared names against a hardcoded "HRMS", matched
 * nothing, and every menu silently vanished for users arriving from the Home portal — their
 * permissions were never the problem.
 */
export const OWN_SUBSYSTEM_ABBREVIATION = "HRMS";
