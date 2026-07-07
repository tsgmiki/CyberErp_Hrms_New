import { useEffect, useMemo, useState } from "react";
import type React from "react";
import type { FormComponentModel } from "@/models";
import { FieldShell } from "./fieldShell";
import { FORM_INPUT_CLASS, FORM_SELECT_CLASS } from "./fieldStyles";
import { formatBackendDate } from "@/components/util/dateFormater";
import {
  ETHIOPIAN_MONTHS,
  ETHIOPIAN_MONTHS_GEEZ,
  ethiopianMonthLength,
  ethiopianToGregorian,
  gregorianToEthiopian,
  parseISODate,
  toISODate,
} from "@/components/util/ethiopianDate";

interface EthParts {
  year: string;
  month: string;
  day: string;
}

const EMPTY: EthParts = { year: "", month: "", day: "" };

/**
 * Dual Gregorian + Ethiopian (Ge'ez) date picker. The canonical value stays Gregorian
 * (yyyy-mm-dd, emitted via onChange like a normal date input); the Ethiopian side is kept
 * in sync both ways — editing either calendar updates the other automatically.
 */
const DualDateField = ({
  error,
  required = false,
  name,
  label,
  disabled = false,
  labelWidth,
  value = "",
  onKeyDown,
  onChange,
  colSpan,
  layout,
}: FormComponentModel) => {
  const gregValue = value ? formatBackendDate(value) : "";

  const ethFromValue = useMemo<EthParts>(() => {
    const g = parseISODate(gregValue);
    if (!g) return EMPTY;
    const e = gregorianToEthiopian(g.year, g.month, g.day);
    return { year: String(e.year), month: String(e.month), day: String(e.day) };
  }, [gregValue]);

  const [eth, setEth] = useState<EthParts>(ethFromValue);

  // Keep the Ethiopian inputs in sync when the Gregorian value changes externally.
  useEffect(() => setEth(ethFromValue), [ethFromValue]);

  const emitGregorian = (iso: string) => {
    onChange?.({ target: { name, value: iso } } as unknown as React.ChangeEvent<HTMLInputElement>);
  };

  const handleGregChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange?.(e);
  };

  const handleEthChange = (patch: Partial<EthParts>) => {
    const next = { ...eth, ...patch };
    setEth(next);
    const y = Number(next.year);
    const m = Number(next.month);
    const d = Number(next.day);
    if (next.year && next.month && next.day && y > 0 && m >= 1 && m <= 13 && d >= 1) {
      const maxDay = ethiopianMonthLength(y, m);
      const clampedDay = Math.min(d, maxDay);
      const g = ethiopianToGregorian(y, m, clampedDay);
      emitGregorian(toISODate(g));
    }
  };

  const maxDay = eth.year && eth.month ? ethiopianMonthLength(Number(eth.year), Number(eth.month)) : 30;
  const dayOptions = Array.from({ length: maxDay }, (_, i) => i + 1);

  return (
    <FieldShell
      name={name}
      label={label}
      required={required}
      labelWidth={labelWidth}
      colSpan={colSpan}
      error={error}
      layout={layout ?? "horizontal"}
    >
      <div className="space-y-2">
        {/* Gregorian */}
        <div>
          <span className="mb-1 block text-[11px] font-medium uppercase tracking-wide text-muted">
            Gregorian
          </span>
          <input
            className={FORM_INPUT_CLASS}
            name={name}
            id={name}
            type="date"
            disabled={disabled}
            value={gregValue}
            onChange={handleGregChange}
            onKeyDown={onKeyDown}
          />
        </div>

        {/* Ethiopian */}
        <div>
          <span className="mb-1 block text-[11px] font-medium uppercase tracking-wide text-muted">
            Ethiopian (ግዕዝ)
          </span>
          <div className="grid grid-cols-[minmax(0,4.5rem)_minmax(0,1fr)_minmax(0,5rem)] gap-2">
            <select
              className={FORM_SELECT_CLASS}
              disabled={disabled}
              value={eth.day}
              onChange={(e) => handleEthChange({ day: e.target.value })}
              aria-label="Ethiopian day"
            >
              <option value="">Day</option>
              {dayOptions.map((d) => (
                <option key={d} value={d}>
                  {d}
                </option>
              ))}
            </select>
            <select
              className={FORM_SELECT_CLASS}
              disabled={disabled}
              value={eth.month}
              onChange={(e) => handleEthChange({ month: e.target.value })}
              aria-label="Ethiopian month"
            >
              <option value="">Month</option>
              {ETHIOPIAN_MONTHS.map((m, i) => (
                <option key={m} value={i + 1}>
                  {ETHIOPIAN_MONTHS_GEEZ[i]} — {m}
                </option>
              ))}
            </select>
            <input
              className={FORM_INPUT_CLASS}
              type="number"
              min={1}
              disabled={disabled}
              placeholder="Year"
              value={eth.year}
              onChange={(e) => handleEthChange({ year: e.target.value })}
              aria-label="Ethiopian year"
            />
          </div>
        </div>
      </div>
    </FieldShell>
  );
};

export default DualDateField;
