import { signal } from "@preact/signals-react";

/**
 * The subsystem this session is currently scoped to, chosen on the landing page.
 *
 * `abbreviation` is the SCOPE KEY (Core.Subsystem.Abbreviation — "HRMS", "SSMS"); `name` is only a
 * display label. They are stored separately on purpose: the sidebar used to scope on the name, and
 * when the catalogue renamed "HRMS" to "Human Resource Management System" every menu vanished.
 *
 * Empty when the user arrives straight from the Home portal — consumers fall back to this
 * application's own abbreviation (OWN_SUBSYSTEM_ABBREVIATION).
 */
export const ModuleData = signal({} as { abbreviation?: string; name?: string });
