"use client";
import { memo, useEffect, useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { Search, Lock } from "lucide-react";
import { getEmployeeOptions } from "@/services/admin/employeeOptions";

const INPUT = "w-full rounded-md border border-border bg-card px-2.5 py-1.5 pr-8 text-sm text-foreground focus:border-primary focus:outline-none disabled:opacity-60";

/**
 * Role-scoped, searchable employee picker. The SERVER decides what the caller may see:
 * HR admin → every employee; manager → their unit + child units; employee → locked to themselves
 * (auto-selected, read-only). PERFORMANCE: debounced remote search returning at most 20 projected
 * rows — the employee table is never bulk-loaded into the browser.
 */
function EmployeePickerBase({
  value,
  displayValue,
  onSelect,
  disabled,
  placeholder,
  excludeId,
}: {
  value?: string;
  /** Shown when closed (e.g. the saved record's employee name). */
  displayValue?: string;
  onSelect: (id: string, name: string) => void;
  disabled?: boolean;
  placeholder?: string;
  /** Employee to omit from the options (server-filtered) — e.g. the appraisee in a peer-reviewer picker. */
  excludeId?: string;
}) {
  const { t } = useTranslation();
  const [term, setTerm] = useState("");
  const [query, setQuery] = useState("");
  const [open, setOpen] = useState(false);
  const boxRef = useRef<HTMLDivElement>(null);

  // Debounce keystrokes (300 ms) so typing never fires a request per key.
  useEffect(() => {
    const h = setTimeout(() => setQuery(term), 300);
    return () => clearTimeout(h);
  }, [term]);

  const { data } = useQuery({
    queryKey: ["employeeOptions", query, excludeId ?? ""],
    queryFn: () => getEmployeeOptions(query || undefined, excludeId),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });

  const isSelf = data?.scope === "Self";
  const selfId = data?.self?.id;
  const selfName = data?.self?.name;

  // Self-service: lock the field to the caller and auto-select them once.
  useEffect(() => {
    if (isSelf && selfId && selfName && value !== selfId) onSelect(selfId, selfName);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isSelf, selfId]);

  // Close the list on outside click.
  useEffect(() => {
    const close = (e: MouseEvent) => {
      if (!boxRef.current?.contains(e.target as Node)) setOpen(false);
    };
    document.addEventListener("mousedown", close);
    return () => document.removeEventListener("mousedown", close);
  }, []);

  if (isSelf) {
    return (
      <div className="relative">
        <input className={INPUT} value={selfName ?? displayValue ?? ""} disabled readOnly title={t("Locked to your own record") ?? ""} />
        <Lock className="pointer-events-none absolute right-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted" />
      </div>
    );
  }

  return (
    <div ref={boxRef} className="relative">
      <input
        className={INPUT}
        disabled={disabled}
        value={open ? term : (displayValue ?? "")}
        placeholder={placeholder ?? (t("Search employee…") ?? "")}
        onFocus={() => { setTerm(""); setOpen(true); }}
        onChange={(e) => setTerm(e.target.value)}
      />
      <Search className="pointer-events-none absolute right-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted" />
      {open && !disabled && (
        <div className="absolute z-30 mt-1 max-h-60 w-full overflow-auto rounded-md border border-border bg-card shadow-lg">
          {(data?.options ?? []).length === 0 ? (
            <p className="px-3 py-2 text-xs text-muted">{t("No matching employees.")}</p>
          ) : (
            (data?.options ?? []).map((o) => (
              <button
                key={o.id}
                type="button"
                onMouseDown={(e) => e.preventDefault()}
                onClick={() => { onSelect(o.id, o.name); setOpen(false); }}
                className={`flex w-full items-center justify-between px-3 py-1.5 text-left text-sm hover:bg-secondary/40 ${o.id === value ? "bg-primary/10 text-primary" : "text-foreground"}`}
              >
                <span className="truncate">{o.name}</span>
                <span className="ml-2 shrink-0 text-xs text-muted">{o.employeeNumber}</span>
              </button>
            ))
          )}
        </div>
      )}
    </div>
  );
}

export default memo(EmployeePickerBase);
