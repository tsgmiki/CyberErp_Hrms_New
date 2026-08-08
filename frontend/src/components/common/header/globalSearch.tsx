"use client";
import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import {
  ArrowDown,
  ArrowUp,
  Building2,
  CalendarCheck,
  CornerDownLeft,
  Loader2,
  Search,
  Users,
  type LucideIcon,
} from "lucide-react";
import { searchGlobal, type SearchResultItem } from "@/services/search/globalSearch";
import { useMenuModules } from "@/components/menu/hooks/useMenuModules";
import getAllOperation from "@/services/admin/operation/getAll";
import { parameterInitialData } from "@/constants/initialization";
import { normalizeRoutePath as norm, resolveRouteKey } from "@/utils/routeMatch";
import { buildRecordRoute } from "@/routes/entityRoutes";

const MIN_LEN = 2;
const DEBOUNCE_MS = 250;

/** Maps the backend's lucide icon name → the component (falls back to Search). */
const ICONS: Record<string, LucideIcon> = {
  users: Users,
  "building-2": Building2,
  "calendar-check": CalendarCheck,
};

/** Bolds the matched span of `text` (case-insensitive) without dangerouslySetInnerHTML. */
function highlight(text: string, term: string) {
  const i = text.toLowerCase().indexOf(term.toLowerCase());
  if (i < 0 || !term) return text;
  return (
    <>
      {text.slice(0, i)}
      <mark className="bg-transparent font-semibold text-foreground">{text.slice(i, i + term.length)}</mark>
      {text.slice(i + term.length)}
    </>
  );
}

/**
 * Global search command palette (⌘K / header search). Debounced, categorized results with full
 * keyboard navigation — the SAP/Dynamics launchpad-search pattern. Renders the card only; the header
 * owns the backdrop overlay.
 */
function GlobalSearch({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const inputRef = useRef<HTMLInputElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  const [input, setInput] = useState("");
  const [term, setTerm] = useState("");
  const [active, setActive] = useState(0);

  // Debounce: only the settled input (after DEBOUNCE_MS of quiet) becomes the query term → far fewer calls.
  useEffect(() => {
    const id = setTimeout(() => setTerm(input.trim()), DEBOUNCE_MS);
    return () => clearTimeout(id);
  }, [input]);

  useEffect(() => inputRef.current?.focus(), []);

  const enabled = term.length >= MIN_LEN;
  const { data, isFetching } = useQuery({
    queryKey: ["globalSearch", term],
    queryFn: ({ signal }) => searchGlobal(term, 5, signal),
    enabled,
    staleTime: 30_000,
    placeholderData: keepPreviousData, // keep prior results visible while the next page loads (no flicker)
  });

  // Don't surface results for modules the user can't open: mirror the route PermissionGate
  // (granted menu links + full operation catalog, both already cached). Fails OPEN — while the
  // menu/catalog load or error, everything shows; the backend stays the real data-access wall.
  const { modules, isLoading: menuLoading } = useMenuModules();
  const { data: catalog, isLoading: catLoading, isError: catError } = useQuery({
    queryKey: ["operationCatalog"],
    queryFn: () => getAllOperation({ ...parameterInitialData, take: 1000 }),
    staleTime: 10 * 60 * 1000,
  });
  const grantedSet = useMemo(() => {
    const s = new Set<string>();
    (modules ?? []).forEach((m) =>
      (m.operations ?? []).forEach((op) => { if (op.canView !== false && op.link) s.add(norm(op.link)); }),
    );
    return s;
  }, [modules]);
  const catalogSet = useMemo(() => {
    const s = new Set<string>();
    (catalog?.data ?? []).forEach((op) => { if (op.link) s.add(norm(op.link)); });
    return s;
  }, [catalog]);
  const gatesReady = !menuLoading && !catLoading && !catError;
  const reachable = (route: string) => {
    if (!gatesReady) return true; // optimistic while gates load / on error
    // Resolve to the owning operation so a record-bearing result ("/branch/{guid}") is gated by
    // its list operation instead of passing as an unrecognised non-menu page.
    const key = resolveRouteKey(route, catalogSet);
    return key === undefined || grantedSet.has(key); // non-menu pages pass; gated pages need a grant
  };

  const groups = (enabled ? data?.groups ?? [] : []).filter((g) => reachable(g.items[0]?.route ?? ""));
  // Flat list of items in display order — the target of arrow-key navigation.
  const flat = useMemo(() => groups.flatMap((g) => g.items), [groups]);

  // Reset the highlight to the top whenever the result set changes.
  useEffect(() => setActive(0), [term, data]);

  // Keep the active row scrolled into view during keyboard navigation.
  useEffect(() => {
    listRef.current?.querySelector<HTMLElement>(`[data-idx="${active}"]`)?.scrollIntoView({ block: "nearest" });
  }, [active]);

  const select = (item?: SearchResultItem) => {
    if (!item) return;
    onClose();
    // Go straight to the record, not to its list. Providers return the module's base route
    // ("/employee") plus the record id, and buildRecordRoute turns that into "/employee/{guid}"
    // for every module that has a record URL — so this is uniform across all search categories and
    // needs no backend change when a new provider is added. Modules still on a flat route fall
    // back to their list, exactly as before.
    navigate(buildRecordRoute(item.route, item.id));
  };

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setActive((a) => Math.min(a + 1, Math.max(flat.length - 1, 0)));
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setActive((a) => Math.max(a - 1, 0));
    } else if (e.key === "Enter") {
      e.preventDefault();
      select(flat[active]);
    } else if (e.key === "Escape") {
      e.preventDefault();
      onClose();
    }
  };

  const showEmpty = enabled && !isFetching && flat.length === 0;
  let idx = -1; // running flat index assigned across groups while rendering

  return (
    <div
      className="mx-4 flex max-h-[70vh] w-full max-w-xl flex-col overflow-hidden rounded-xl border border-border bg-card shadow-2xl"
      onClick={(e) => e.stopPropagation()}
      role="dialog"
      aria-label={t("Global search")}
    >
      {/* Search input */}
      <div className="flex items-center gap-3 border-b border-border px-4">
        {isFetching ? (
          <Loader2 className="h-4 w-4 shrink-0 animate-spin text-muted-foreground" />
        ) : (
          <Search className="h-4 w-4 shrink-0 text-muted-foreground" />
        )}
        <input
          ref={inputRef}
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={onKeyDown}
          placeholder={t("Search employees, departments, leave requests…")}
          className="h-12 flex-1 bg-transparent text-sm text-foreground outline-none placeholder:text-muted-foreground"
          aria-label={t("Search")}
          role="combobox"
          aria-expanded={flat.length > 0}
          autoComplete="off"
          spellCheck={false}
        />
        <kbd className="hidden shrink-0 rounded border border-border px-1.5 py-0.5 text-[10px] font-medium text-muted-foreground sm:inline">
          Esc
        </kbd>
      </div>

      {/* Results */}
      <div ref={listRef} className="min-h-0 flex-1 overflow-y-auto py-2" role="listbox">
        {!enabled && (
          <p className="px-4 py-6 text-center text-xs text-muted-foreground">
            {t("Type at least 2 characters to search across employees, departments and leave requests.")}
          </p>
        )}

        {showEmpty && (
          <p className="px-4 py-6 text-center text-sm text-muted-foreground">
            {t("No results for")} <span className="font-medium text-foreground">“{term}”</span>
          </p>
        )}

        {groups.map((group) => {
          const Icon = ICONS[group.icon] ?? Search;
          return (
            <div key={group.category} className="mb-1">
              <div className="flex items-center gap-2 px-4 pb-1 pt-2 text-[11px] font-semibold uppercase tracking-wide text-muted-foreground">
                <Icon className="h-3.5 w-3.5" />
                {t(group.category)}
                <span className="ml-auto rounded-full bg-secondary px-1.5 text-[10px] font-medium">{group.items.length}</span>
              </div>
              {group.items.map((item) => {
                idx += 1;
                const i = idx;
                const isActive = i === active;
                return (
                  <button
                    key={item.id}
                    type="button"
                    data-idx={i}
                    role="option"
                    aria-selected={isActive}
                    onMouseMove={() => setActive(i)}
                    onClick={() => select(item)}
                    className={`flex w-full items-center gap-3 px-4 py-2 text-left transition-colors ${
                      isActive ? "bg-primary/10" : "hover:bg-secondary/60"
                    }`}
                  >
                    <span
                      className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-md ${
                        isActive ? "bg-primary/15 text-primary" : "bg-secondary text-muted-foreground"
                      }`}
                    >
                      <Icon className="h-4 w-4" />
                    </span>
                    <span className="min-w-0 flex-1">
                      <span className="block truncate text-sm text-foreground">{highlight(item.title, term)}</span>
                      {item.subtitle && (
                        <span className="block truncate text-xs text-muted-foreground">{item.subtitle}</span>
                      )}
                    </span>
                    {isActive && <CornerDownLeft className="h-3.5 w-3.5 shrink-0 text-muted-foreground" />}
                  </button>
                );
              })}
            </div>
          );
        })}
      </div>

      {/* Footer hint bar */}
      <div className="flex items-center gap-4 border-t border-border bg-secondary/20 px-4 py-2 text-[11px] text-muted-foreground">
        <span className="flex items-center gap-1">
          <ArrowUp className="h-3 w-3" />
          <ArrowDown className="h-3 w-3" />
          {t("Navigate")}
        </span>
        <span className="flex items-center gap-1">
          <CornerDownLeft className="h-3 w-3" />
          {t("Open")}
        </span>
        <span className="ml-auto">{t("Esc to close")}</span>
      </div>
    </div>
  );
}

export default GlobalSearch;
