import { format, getWeek, formatISO } from 'date-fns';
import { startOfDay, endOfDay, startOfWeek, endOfWeek, startOfMonth, endOfMonth } from 'date-fns';
import { toZonedTime, fromZonedTime } from 'date-fns-tz';

export default function formatDate(date: Date | string) {
  const d = new Date(date);
  return format(d, 'yyyy/MM/dd');
}

export function displayFormatDate(date: Date | string) {
  const d = new Date(date);
  return format(d, 'yyyy-MM-dd');
}

// Helper function to format date from backend format like "2026-02-18T21:00:00 UTC" or "2026-04-15 21:00:00"
export function formatBackendDate(dateStr: string | undefined | null): string {
  if (!dateStr) return "-";
  try {
    let datePart: string;
    if (dateStr.includes('T')) {
      datePart = dateStr.split('T')[0];
    } else if (dateStr.includes(' ')) {
      datePart = dateStr.split(' ')[0];
    } else {
      datePart = dateStr;
    }
    if (datePart) {
      return datePart;
    }
    return displayFormatDate(dateStr);
  } catch {
    return "-";
  }
}

export function getWeekNo(date: Date | string) {
  const d = new Date(date);
  return getWeek(d);
}

export function getEatDate(date: Date | string) {
  const d = new Date(date);
  const zoned = fromZonedTime(d, 'Africa/Nairobi');
  return formatISO(zoned);
}

export function getMonthName(date: Date | string) {
  const d = new Date(date);
  return format(d, 'MMMM');
}

export function getAllDates() {
  const now = new Date();
  const tz = 'Africa/Nairobi';

  const todayStart = fromZonedTime(startOfDay(toZonedTime(now, tz)), tz);
  const todayEnd = fromZonedTime(endOfDay(toZonedTime(now, tz)), tz);

  const weekStart = fromZonedTime(startOfWeek(toZonedTime(now, tz), { weekStartsOn: 1 }), tz); // Monday
  const weekEnd = fromZonedTime(endOfWeek(toZonedTime(now, tz), { weekStartsOn: 1 }), tz);

  const monthStart = fromZonedTime(startOfMonth(toZonedTime(now, tz)), tz);
  const monthEnd = fromZonedTime(endOfMonth(toZonedTime(now, tz)), tz);

  return {
    today: { start: formatISO(todayStart), end: formatISO(todayEnd) },
    week: { start: formatISO(weekStart), end: formatISO(weekEnd) },
    month: { start: formatISO(monthStart), end: formatISO(monthEnd) },
  };
}
