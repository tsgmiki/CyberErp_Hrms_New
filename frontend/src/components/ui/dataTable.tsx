import type { DataTableColumnModel, DataTableModel } from "@/models";
import { groupTableRows } from "@/components/common/dataTableProvider/dataTableGrouping";
import { DataTableCheckbox } from "@/components/common/dataTableProvider/dataTableCheckbox";
import { GroupTableHeader } from "@/components/common/dataTableProvider/groupTableHeader";
import {
  getRowId,
  isAllPageSelected,
  isSomePageSelected,
  toggleAllSelection,
  toggleRowSelection,
} from "@/components/common/dataTableProvider/dataTableSelection";
import { ArrowDown, ArrowUp, ArrowUpDown, ChevronDown, ChevronRight } from "lucide-react";
import { Fragment, useMemo, useState, type ReactNode } from "react";
import { useTranslation } from "react-i18next";
import { isString } from "@tiptap/react";

function SortIcon({
  columnName,
  sortCol,
  dir,
}: {
  columnName?: string;
  sortCol?: string;
  dir?: string;
}) {
  if (sortCol !== columnName) {
    return <ArrowUpDown size={12} className="opacity-50" />;
  }
  const normalized = String(dir ?? "").toUpperCase();
  if (normalized === "DESC") return <ArrowDown size={12} />;
  return <ArrowUp size={12} />;
}

const DataTable = ({
  data,
  columns,
  sortHandler,
  showSummary,
  param,
  checkBox,
  checkList = [],
  checkHandler,
  groupBy,
  getGroupLabel,
  GetChildren,
  rowIdKey,
  key: keyField,
}: DataTableModel) => {
  const rowKeyField = rowIdKey ?? keyField ?? "id";
  const { t } = useTranslation();
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string>>(new Set());
  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());

  const responsiveClass = (responsive?: DataTableColumnModel["responsive"]) => {
    if (responsive === "lg") return "hidden lg:table-cell";
    if (responsive === "md") return "hidden md:table-cell";
    if (responsive === "sm") return "hidden sm:table-cell";
    return "";
  };

  const rows = data ?? [];
  const pageRowIds = useMemo(
    () => rows.map((item, index) => getRowId(item, index, rowKeyField)),
    [rows, rowKeyField],
  );

  const groups = useMemo(
    () =>
      groupTableRows(rows, groupBy, getGroupLabel
        ? (key, groupRows) => getGroupLabel(key, groupRows)
        : undefined),
    [rows, groupBy, getGroupLabel],
  );

  const allSelected = isAllPageSelected(checkList, pageRowIds);
  const someSelected = isSomePageSelected(checkList, pageRowIds);

  const summary = (columns ?? []).reduce<Record<string, string>>((acc, col, index) => {
    const colName = col.name as string;
    const hasNumbers = rows.some((record) => typeof record[colName] === "number");

    if (hasNumbers) {
      const colValue = rows.reduce(
        (total, record) =>
          total +
          (!Number.isNaN(Number(record[colName])) ? Number(record[colName]) : 0),
        0,
      );
      acc[colName] = Number(colValue).toLocaleString();
    } else {
      acc[colName] = index === 0 ? t("Summary") : "";
    }
    return acc;
  }, {});

  const colSpan =
    (columns?.length ?? 0) + (checkBox ? 1 : 0) + (GetChildren ? 1 : 0);

  const toggleGroup = (groupKey: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(groupKey)) next.delete(groupKey);
      else next.add(groupKey);
      return next;
    });
  };

  const toggleExpandRow = (rowId: string) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(rowId)) next.delete(rowId);
      else next.add(rowId);
      return next;
    });
  };

  const renderDataRow = (item: Record<string, unknown>, rowIndex: number) => {
    const rowId = getRowId(item, rowIndex, rowKeyField);
    const isChecked = checkList.includes(rowId);
    const canExpand = Boolean(GetChildren && item.hasChildren);
    const isExpanded = expandedRows.has(rowId);

    return (
      <Fragment key={rowId}>
        <tr
          className={`border-b border-border transition-colors hover:bg-secondary/50 ${
            isChecked
              ? "bg-[color-mix(in_srgb,var(--primary)_8%,var(--card))]"
              : rowIndex % 2 === 1
                ? "bg-card/40"
                : ""
          }`}
        >
          {checkBox && (
            <td className="w-11 px-2 py-3 align-middle">
              <DataTableCheckbox
                checked={isChecked}
                onChange={(checked) =>
                  checkHandler?.(toggleRowSelection(checkList, rowId, checked))
                }
                ariaLabel={t("Select row")}
              />
            </td>
          )}

          {GetChildren && (
            <td className="w-10 p-3">
              {canExpand ? (
                <button
                  type="button"
                  onClick={() => toggleExpandRow(rowId)}
                  className="rounded p-1 text-muted transition-colors hover:bg-muted/60 hover:text-foreground"
                  aria-expanded={isExpanded}
                >
                  {isExpanded ? (
                    <ChevronDown size={16} />
                  ) : (
                    <ChevronRight size={16} />
                  )}
                </button>
              ) : null}
            </td>
          )}

          {columns?.map((col) => (
            <td
              key={col.name}
              className={`p-3 ${responsiveClass(col.responsive)} ${col.width ?? "w-auto"}`}
            >
              {typeof col.render !== "undefined"
                ? col.render((item[col.name as string] ?? "") as string, item)
                : (item[col.name as string] as ReactNode)}
            </td>
          ))}
        </tr>

        {canExpand && isExpanded && (
          <tr className="border-b border-border bg-muted/20">
            <td colSpan={colSpan} className="p-3">
              <div className="rounded-lg border border-border bg-card p-3">
                {GetChildren?.(item)}
              </div>
            </td>
          </tr>
        )}
      </Fragment>
    );
  };

  const renderBodyRows = (bodyRows: Record<string, unknown>[], startIndex = 0) =>
    bodyRows.map((item, idx) => renderDataRow(item, startIndex + idx));

  return (
    <div className="w-full overflow-hidden rounded-xl border border-border bg-card transition-colors duration-300">
      <div className="max-h-[min(70vh,720px)] overflow-auto">
        <table className="w-full min-w-max table-auto text-left">
          <thead className="sticky top-0 z-10 bg-muted/80 text-xs font-semibold uppercase tracking-wider text-table-header backdrop-blur-sm">
            <tr>
              {checkBox && (
                <th scope="col" className="w-11 border-b border-border px-2 py-3">
                  <DataTableCheckbox
                    checked={allSelected}
                    indeterminate={someSelected && !allSelected}
                    onChange={(checked) =>
                      checkHandler?.(
                        toggleAllSelection(checkList, pageRowIds, checked),
                      )
                    }
                    ariaLabel={t("Select all")}
                  />
                </th>
              )}
              {GetChildren && (
                <th scope="col" className="w-10 border-b border-border p-3" />
              )}
              {columns?.map((val) => (
                <th
                  key={val.name}
                  scope="col"
                  className={`border-b border-border p-3 leading-none ${responsiveClass(val.responsive)} ${val.width ?? "w-auto"}`}
                >
                  <span className="flex items-center justify-between gap-2">
                    {isString(val.label) ? t(val.label) : val.label}
                    {val.sort && (
                      <button
                        type="button"
                        onClick={() => sortHandler?.(val.name as string)}
                        className={`rounded p-1 transition-colors hover:bg-muted/60 ${
                          param?.sortCol === val.name ? "text-primary" : ""
                        }`}
                        aria-label={t("Sort")}
                      >
                        <SortIcon
                          columnName={val.name}
                          sortCol={param?.sortCol}
                          dir={param?.dir}
                        />
                      </button>
                    )}
                  </span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="text-sm text-foreground">
            {groups
              ? groups.map((group) => {
                  const isCollapsed = collapsedGroups.has(group.key);
                  return (
                    <Fragment key={`group-${group.key}`}>
                      <tr className="border-b border-border">
                        <td colSpan={colSpan} className="p-0">
                          <GroupTableHeader
                            label={group.label}
                            count={group.rows.length}
                            isCollapsed={isCollapsed}
                            onToggle={() => toggleGroup(group.key)}
                            variant="bar"
                          />
                        </td>
                      </tr>
                      {!isCollapsed && renderBodyRows(group.rows)}
                    </Fragment>
                  );
                })
              : renderBodyRows(rows)}

            {showSummary && (
              <tr className="border-t border-border font-semibold" key="summary">
                {checkBox && <td className="p-3" />}
                {GetChildren && <td className="p-3" />}
                {columns?.map((col) => (
                  <td
                    key={col.name}
                    className={`p-3 ${responsiveClass(col.responsive)}`}
                  >
                    {summary[col.name as string]}
                  </td>
                ))}
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

DataTable.defaultProps = {
  pagination: "Visible",
};

export default DataTable;
