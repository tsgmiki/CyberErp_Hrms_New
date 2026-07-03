import type { ReactNode } from "react";

export default interface DataTableColumnModel {
  name?: string;
  label?: any;
  key?: string;
  sort?: boolean;
  responsive?: "sm" | "md" | "lg";
  width?: string;
  render?: (text: string, record: any) => ReactNode;
  /** Card title field in grid view */
  gridPrimary?: boolean;
  /** Opt in: show in the card body before “Show more” (default is header + highlights only) */
  gridPreview?: boolean;
  /** Excluded from grid cards */
  gridOmit?: boolean;
  /** Hide the field label above the primary value in grid header */
  gridHideLabel?: boolean;
  /** Pin field to the card header strip in grid view */
  gridHighlight?: boolean;
  /** Grid-only cell renderer (falls back to `render`) */
  gridRender?: (text: string, record: any) => ReactNode;
}
