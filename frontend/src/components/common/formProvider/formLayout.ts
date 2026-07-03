export { FORM_INPUT_CLASS, FORM_TEXTAREA_CLASS } from "@/components/ui/fieldStyles";

export function getFormGridClass(columnsNo = 1, compact = false): string {
  const gap = compact ? "gap-4" : "gap-5";
  const padding = compact ? "p-4" : "p-5";

  const cols =
    columnsNo === 3
      ? "grid-cols-1 md:grid-cols-2 xl:grid-cols-3"
      : columnsNo === 2
        ? "grid-cols-1 md:grid-cols-2"
        : "grid-cols-1";

  return `grid ${cols} ${gap} ${padding}`;
}

export function getFieldCellClass(component: {
  type?: string;
  colSpan?: string;
  hidden?: boolean;
}): string {
  if (component.type === "hidden") return "hidden";
  if (component.type === "break" || component.colSpan === "full") {
    return "col-span-full";
  }
  return "min-w-0";
}
