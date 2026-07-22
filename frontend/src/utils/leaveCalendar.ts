// Client-side mirror of the backend WorkingCalendar (App/Features/Core/Leaves/WorkingCalendar.cs).
// Keeps the Leave Request grid interactive without a round-trip, using the same rules the server
// re-applies authoritatively on submit: a day is worth Full=1 / Half=0.5 / Rest=0 per the active
// work-week configuration, and 0 on an active holiday (fixed date or recurring by month+day).
import type { WorkWeekConfigurationModel, HolidayModel } from "@/models";

const VALUE: Record<string, number> = { Full: 1, Half: 0.5, Rest: 0 };
const MAX_DAYS = 366 * 10; // safety cap so a bad input can never spin forever
const round2 = (n: number) => Math.round(n * 100) / 100;

/** Parse a "YYYY-MM-DD" (or ISO) string to a LOCAL midnight Date; null if blank/invalid. */
export function parseDate(s?: string): Date | null {
  if (!s) return null;
  const [y, m, d] = s.slice(0, 10).split("-").map(Number);
  if (!y || !m || !d) return null;
  return new Date(y, m - 1, d);
}

/** Format a Date as "YYYY-MM-DD" (local, no timezone shift). */
export function formatDate(d: Date): string {
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${m}-${day}`;
}

/**
 * Work value indexed by JS weekday (0=Sun … 6=Sat) from the active work-week configuration.
 * Falls back to a standard Mon–Fri work week (Sat/Sun rest) — identical to the backend fallback.
 */
export function buildWorkValues(config?: WorkWeekConfigurationModel | null): number[] {
  if (!config) return [0, 1, 1, 1, 1, 1, 0]; // Sun, Mon…Fri, Sat
  const v = (s?: string) => VALUE[s ?? "Rest"] ?? 0;
  return [
    v(config.sunday),
    v(config.monday),
    v(config.tuesday),
    v(config.wednesday),
    v(config.thursday),
    v(config.friday),
    v(config.saturday),
  ];
}

export interface HolidaySets {
  exact: Set<string>; // "YYYY-MM-DD" for fixed-date holidays
  recurring: Set<string>; // "M-D" for holidays that repeat every year
}

export function buildHolidaySets(holidays: HolidayModel[]): HolidaySets {
  const exact = new Set<string>();
  const recurring = new Set<string>();
  for (const h of holidays) {
    if (h.isActive === false) continue;
    const iso = (h.date ?? "").slice(0, 10);
    const [y, m, d] = iso.split("-").map(Number);
    if (!m || !d) continue;
    if (h.isRecurring) recurring.add(`${m}-${d}`);
    else if (y) exact.add(iso);
  }
  return { exact, recurring };
}

/** Chargeable value of a single day: 0 on a holiday, otherwise the weekday's work value. */
export function dayValue(date: Date, workValues: number[], holidays: HolidaySets): number {
  if (holidays.exact.has(formatDate(date))) return 0;
  if (holidays.recurring.has(`${date.getMonth() + 1}-${date.getDate()}`)) return 0;
  return workValues[date.getDay()] ?? 0;
}

/**
 * Rule 1 — sum the working-day value across the inclusive range [start, end], excluding rest days
 * and holidays. A half-day request is a single day charged at 0.5 × that day's value.
 */
export function countLeaveDays(
  start: Date,
  end: Date,
  halfDay: boolean,
  workValues: number[],
  holidays: HolidaySets,
): number {
  if (halfDay) return round2(dayValue(start, workValues, holidays) * 0.5);
  if (end < start) return 0;
  let total = 0;
  const d = new Date(start);
  for (let i = 0; d <= end && i < MAX_DAYS; i++) {
    total += dayValue(d, workValues, holidays);
    d.setDate(d.getDate() + 1);
  }
  return round2(total);
}

/**
 * Rule 2 — from `start`, walk forward accumulating working-day value until it reaches `leaveDays`,
 * skipping rest days and holidays; returns the last date that contributed (the final active date).
 */
export function addLeaveDays(
  start: Date,
  leaveDays: number,
  workValues: number[],
  holidays: HolidaySets,
): Date | null {
  if (!(leaveDays > 0)) return null;
  let cumulative = 0;
  let last: Date | null = null;
  const d = new Date(start);
  for (let i = 0; i < MAX_DAYS; i++) {
    const v = dayValue(d, workValues, holidays);
    if (v > 0) {
      cumulative += v;
      last = new Date(d);
      if (cumulative >= leaveDays - 1e-9) return last;
    }
    d.setDate(d.getDate() + 1);
  }
  return last;
}
