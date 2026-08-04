/** Org-chart colors + per-theme tokens. Kept free of any renderer specifics so the same values
 * drive the on-screen SVG and the (rasterized-from-SVG) PDF. One hue per unit type. */

export const UNIT_ORDER = ["BusinessUnit", "Directorate", "Division", "Department", "Team", "Branch"] as const;

export const HUE: Record<string, { light: string; dark: string }> = {
  BusinessUnit: { light: "#2a78d6", dark: "#4f9bf0" },
  Directorate: { light: "#1baf7a", dark: "#2ec894" },
  Division: { light: "#e08600", dark: "#f0a12b" },
  Department: { light: "#0f9d58", dark: "#28b56b" },
  Team: { light: "#6a5ae0", dark: "#9085e9" },
  Branch: { light: "#64748b", dark: "#94a3b8" },
};
const FALLBACK = { light: "#475569", dark: "#94a3b8" };

export function hueFor(type: string, dark: boolean) {
  return (HUE[type] ?? FALLBACK)[dark ? "dark" : "light"];
}

export function hexToRgba(hex: string, alpha: number) {
  const h = hex.replace("#", "");
  const r = parseInt(h.slice(0, 2), 16);
  const g = parseInt(h.slice(2, 4), 16);
  const b = parseInt(h.slice(4, 6), 16);
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}

export interface OrgTheme {
  dark: boolean;
  canvas: string; // pane background behind the cards
  card: string; // card surface
  cardBorder: string;
  ink: string; // primary text
  muted: string; // secondary text
  chipBg: string; // neutral chip fill
  edge: string; // connector stroke
  dim: number; // opacity applied to non-matching nodes during search
}

export function orgTheme(dark: boolean): OrgTheme {
  return dark
    ? {
        dark,
        canvas: "#141413",
        card: "#211f1d",
        cardBorder: "#38352f",
        ink: "#f4f2ee",
        muted: "#a8a49a",
        chipBg: "#2c2a26",
        edge: "#403c35",
        dim: 0.28,
      }
    : {
        dark,
        canvas: "#f6f6f3",
        card: "#ffffff",
        cardBorder: "#e6e4dc",
        ink: "#1c1b18",
        muted: "#6b6960",
        chipBg: "#f1f0ea",
        edge: "#d3d1c7",
        dim: 0.32,
      };
}
