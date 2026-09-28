"use client";
import { memo, useEffect, useMemo, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Search } from "lucide-react";

/** One selectable row. `hint` is the dim right-hand text (a username, a code). */
export interface SearchableOption {
  id: string;
  label: string;
  hint?: string;
}

interface Props {
  /** Stable cache key prefix — the search term is appended. */
  queryKey: string;
  /** Fetch matching options for a term. Called debounced, never per keystroke. */
  fetchOptions: (term: string) => Promise<SearchableOption[]>;
  onSelect: (option: SearchableOption) => void;
  placeholder: string;
  /** Already-chosen ids, hidden from the list so a pick can never be a no-op. */
  excludeIds?: string[];
  disabled?: boolean;
  className?: string;
}

const INPUT =
  "w-full rounded-md border border-border bg-card px-2.5 py-1.5 pr-8 text-sm text-foreground "
  + "focus:border-primary focus:outline-none disabled:opacity-60";

/**
 * A type-to-search dropdown backed by a REMOTE query.
 *
 * <p>⚠️ Built because the `&lt;select&gt;` it replaces was not only unsearchable, it was
 * incomplete: a plain option list has to be bulk-loaded, so the call sites capped themselves at
 * the first 100 rows and quietly made everybody past that unselectable. Searching server-side
 * removes the cap along with the scrolling — the list is short because the term is narrow, not
 * because it was truncated.</p>
 *
 * <p>This is an ADD control, not a bound field: it clears itself after each pick, because the
 * caller is collecting several values rather than holding one. That is why there is no `value`
 * prop — a picker that kept showing the last thing chosen would suggest it was still selected.</p>
 *
 * <p>Keyboard: ↑/↓ move, Enter picks, Escape closes. A "searchable dropdown" that can only be
 * driven with a mouse has kept the worst half of the control it replaced.</p>
 */
function SearchableSelectBase({
  queryKey,
  fetchOptions,
  onSelect,
  placeholder,
  excludeIds = [],
  disabled,
  className = "",
}: Props) {
  const { t } = useTranslation();
  const [term, setTerm] = useState("");
  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(0);
  const boxRef = useRef<HTMLDivElement>(null);
  const listRef = useRef<HTMLDivElement>(null);

  // 300 ms, matching EmployeePicker — typing must never fire a request per key.
  useEffect(() => {
    const h = setTimeout(() => setQuery(term), 300);
    return () => clearTimeout(h);
  }, [term]);

  const { data, isFetching } = useQuery({
    queryKey: [queryKey, query],
    queryFn: () => fetchOptions(query),
    enabled: open,
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });

  const options = useMemo(
    () => (data ?? []).filter((o) => !excludeIds.includes(o.id)),
    [data, excludeIds],
  );

  // A narrowed list can be shorter than where the cursor was sitting.
  useEffect(() => setActive(0), [query, data]);

  useEffect(() => {
    const close = (e: MouseEvent) => {
      if (!boxRef.current?.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", close);
    return () => document.removeEventListener("mousedown", close);
  }, []);

  // Keep the highlighted row in view when arrowing past the visible window.
  useEffect(() => {
    if (!open) return;
    listRef.current?.querySelector<HTMLElement>(`[data-index="${active}"]`)
      ?.scrollIntoView({ block: "nearest" });
  }, [active, open]);

  const choose = (option: SearchableOption) => {
    onSelect(option);
    // Reset: this control adds, it does not hold.
    setTerm("");
    setQuery("");
    setOpen(false);
  };

  const onKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Escape") { setOpen(false); return; }
    if (!open && (e.key === "ArrowDown" || e.key === "Enter")) { setOpen(true); return; }
    if (e.key === "ArrowDown") {
      e.preventDefault();
      setActive((i) => Math.min(i + 1, options.length - 1));
    } else if (e.key === "ArrowUp") {
      e.preventDefault();
      setActive((i) => Math.max(i - 1, 0));
    } else if (e.key === "Enter") {
      // Only when something is actually highlighted — Enter on an empty list must not
      // submit the surrounding form by accident.
      e.preventDefault();
      if (options[active]) choose(options[active]);
    }
  };

  return (
    <div ref={boxRef} className={`relative ${className}`}>
      <input
        className={INPUT}
        role="combobox"
        aria-expanded={open}
        aria-autocomplete="list"
        disabled={disabled}
        value={term}
        placeholder={placeholder}
        onFocus={() => setOpen(true)}
        onChange={(e) => { setTerm(e.target.value); setOpen(true); }}
        onKeyDown={onKeyDown}
      />
      <Search className="pointer-events-none absolute right-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted" />

      {open && !disabled && (
        <div
          ref={listRef}
          role="listbox"
          className="absolute z-30 mt-1 max-h-60 w-full overflow-auto rounded-md border border-border bg-card shadow-lg"
        >
          {options.length === 0 ? (
            <p className="px-3 py-2 text-xs text-muted">
              {/* "Still looking" and "nothing matches" are different answers. */}
              {isFetching ? `${t("Searching")}…` : t("No matches.")}
            </p>
          ) : (
            options.map((o, i) => (
              <button
                key={o.id}
                type="button"
                data-index={i}
                role="option"
                aria-selected={i === active}
                // Keep focus in the input so the list does not close before the click lands.
                onMouseDown={(e) => e.preventDefault()}
                onMouseEnter={() => setActive(i)}
                onClick={() => choose(o)}
                className={`flex w-full items-center justify-between px-3 py-1.5 text-left text-sm ${
                  // ⚠️ bg-primary/10, not bg-secondary/40. The latter is NOT a registered
                  // palette variant in this SPA and emits nothing — which for a HOVER tint is a
                  // cosmetic loss, but here it is the keyboard cursor: arrow keys would move a
                  // highlight nobody can see. Verified against the built css.
                  i === active ? "bg-primary/10 text-primary" : "text-foreground"
                }`}
              >
                <span className="truncate">{o.label}</span>
                {o.hint && <span className="ml-2 shrink-0 text-xs text-muted">{o.hint}</span>}
              </button>
            ))
          )}
        </div>
      )}
    </div>
  );
}

export default memo(SearchableSelectBase);
