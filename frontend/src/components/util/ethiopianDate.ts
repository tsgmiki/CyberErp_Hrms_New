/**
 * Ethiopian (Ge'ez) ↔ Gregorian calendar conversion.
 *
 * Uses the Julian Day Number (JDN) as the common pivot. The Gregorian side uses the
 * Fliegel–Van Flandern algorithm; the Ethiopian side uses the Amete-Mihret epoch
 * (Meskerem 1, year 1 = JDN 1724221). Ethiopian leap years are those where year % 4 === 3,
 * which add a 6th day to Pagume (the 13th month). Verified against known date pairs.
 */

export interface EthiopianDate {
  year: number;
  month: number; // 1..13 (13 = Pagume)
  day: number; // 1..30 (Pagume: 1..5, or 1..6 in a leap year)
}

export interface GregorianDate {
  year: number;
  month: number; // 1..12
  day: number; // 1..31
}

/** JDN of Meskerem 1, year 1 (Amete Mihret). */
const ETHIOPIC_EPOCH = 1724221;

/** Ge'ez month names (index 0 = Meskerem … 12 = Pagume). */
export const ETHIOPIAN_MONTHS = [
  "Meskerem",
  "Tikimt",
  "Hidar",
  "Tahsas",
  "Tir",
  "Yekatit",
  "Megabit",
  "Miazia",
  "Ginbot",
  "Sene",
  "Hamle",
  "Nehase",
  "Pagume",
];

export const ETHIOPIAN_MONTHS_GEEZ = [
  "መስከረም",
  "ጥቅምት",
  "ኅዳር",
  "ታኅሣሥ",
  "ጥር",
  "የካቲት",
  "መጋቢት",
  "ሚያዝያ",
  "ግንቦት",
  "ሰኔ",
  "ሐምሌ",
  "ነሐሴ",
  "ጳጉሜ",
];

function gregorianToJDN(y: number, m: number, d: number): number {
  const a = Math.floor((14 - m) / 12);
  const yy = y + 4800 - a;
  const mm = m + 12 * a - 3;
  return (
    d +
    Math.floor((153 * mm + 2) / 5) +
    365 * yy +
    Math.floor(yy / 4) -
    Math.floor(yy / 100) +
    Math.floor(yy / 400) -
    32045
  );
}

function jdnToGregorian(jdn: number): GregorianDate {
  const a = jdn + 32044;
  const b = Math.floor((4 * a + 3) / 146097);
  const c = a - Math.floor((146097 * b) / 4);
  const d = Math.floor((4 * c + 3) / 1461);
  const e = c - Math.floor((1461 * d) / 4);
  const m = Math.floor((5 * e + 2) / 153);
  return {
    day: e - Math.floor((153 * m + 2) / 5) + 1,
    month: m + 3 - 12 * Math.floor(m / 10),
    year: 100 * b + d - 4800 + Math.floor(m / 10),
  };
}

function ethiopicToJDN(year: number, month: number, day: number): number {
  return (
    ETHIOPIC_EPOCH +
    365 * (year - 1) +
    Math.floor(year / 4) +
    30 * (month - 1) +
    (day - 1)
  );
}

function jdnToEthiopic(jdn: number): EthiopianDate {
  const n = jdn - ETHIOPIC_EPOCH; // 0-based days since Meskerem 1, year 1
  const cycle = Math.floor(n / 1461); // completed 4-year cycles
  const r = n - cycle * 1461; // day within the cycle (0..1460)

  // Day counts per year within a cycle: 365, 365, 366 (leap = year%4===3), 365.
  let yearInCycle: number;
  let dayOfYear: number;
  if (r < 365) {
    yearInCycle = 0;
    dayOfYear = r;
  } else if (r < 730) {
    yearInCycle = 1;
    dayOfYear = r - 365;
  } else if (r < 1096) {
    yearInCycle = 2;
    dayOfYear = r - 730;
  } else {
    yearInCycle = 3;
    dayOfYear = r - 1096;
  }

  return {
    year: cycle * 4 + 1 + yearInCycle,
    month: Math.floor(dayOfYear / 30) + 1,
    day: (dayOfYear % 30) + 1,
  };
}

export function gregorianToEthiopian(y: number, m: number, d: number): EthiopianDate {
  return jdnToEthiopic(gregorianToJDN(y, m, d));
}

export function ethiopianToGregorian(y: number, m: number, d: number): GregorianDate {
  return jdnToGregorian(ethiopicToJDN(y, m, d));
}

export function isEthiopianLeapYear(year: number): boolean {
  return year % 4 === 3;
}

/** Days in an Ethiopian month (Pagume = 5, or 6 in a leap year). */
export function ethiopianMonthLength(year: number, month: number): number {
  if (month === 13) return isEthiopianLeapYear(year) ? 6 : 5;
  return 30;
}

/** Parse a yyyy-mm-dd (or ISO) string into Gregorian parts, or null. */
export function parseISODate(value?: string | null): GregorianDate | null {
  if (!value) return null;
  const datePart = value.includes("T") ? value.split("T")[0] : value.trim();
  const m = /^(\d{4})-(\d{1,2})-(\d{1,2})$/.exec(datePart);
  if (!m) return null;
  const year = Number(m[1]);
  const month = Number(m[2]);
  const day = Number(m[3]);
  if (month < 1 || month > 12 || day < 1 || day > 31) return null;
  return { year, month, day };
}

/** Format Gregorian parts as yyyy-mm-dd (zero-padded). */
export function toISODate({ year, month, day }: GregorianDate): string {
  const p = (n: number) => String(n).padStart(2, "0");
  return `${String(year).padStart(4, "0")}-${p(month)}-${p(day)}`;
}
