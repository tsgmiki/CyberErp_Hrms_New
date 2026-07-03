export function getRowId(
  item: Record<string, unknown>,
  index: number,
  rowKey = "id",
): string {
  const id = item[rowKey] ?? item.id ?? item._id;
  if (id != null && id !== "") return String(id);
  return `row-${index}`;
}

export function toggleRowSelection(
  selectedIds: string[],
  rowId: string,
  checked: boolean,
): string[] {
  if (checked) {
    return selectedIds.includes(rowId) ? selectedIds : [...selectedIds, rowId];
  }
  return selectedIds.filter((id) => id !== rowId);
}

export function toggleAllSelection(
  selectedIds: string[],
  rowIds: string[],
  checked: boolean,
): string[] {
  if (!checked) {
    return selectedIds.filter((id) => !rowIds.includes(id));
  }
  const merged = new Set([...selectedIds, ...rowIds]);
  return Array.from(merged);
}

export function isAllPageSelected(
  selectedIds: string[],
  rowIds: string[],
): boolean {
  return rowIds.length > 0 && rowIds.every((id) => selectedIds.includes(id));
}

export function isSomePageSelected(
  selectedIds: string[],
  rowIds: string[],
): boolean {
  return rowIds.some((id) => selectedIds.includes(id));
}
