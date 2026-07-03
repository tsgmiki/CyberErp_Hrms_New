/**
 * Central env-backed configuration for white-label / multi-project deployments.
 * Import from here instead of `import.meta.env` scattered across the app.
 */
export const appConfig = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL as string,
  appName: (import.meta.env.VITE_APP_NAME as string | undefined) ?? "Cyber HRMS",
  defaultLocale: (import.meta.env.VITE_DEFAULT_LOCALE as string | undefined) ?? "en",
} as const;
