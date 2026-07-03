import type { DataTableColumnModel, DataTableModel } from "@/models";
import { useCallback, useMemo, useState } from "react";
import DataTableChrome from "./dataTableChrome";
import { buildGridColumnLayout } from "./gridColumnLayout";
import GridDataTableCard from "./gridDataTableCard";
import { getRowId, toggleRowSelection } from "./dataTableSelection";
import { groupTableRows } from "./dataTableGrouping";
import { GroupBody, GroupSection, GroupTableHeader } from "./groupTableHeader";

interface GridDataTableProviderProps {
  dataTable: DataTableModel;
}

function GridDataTableProvider({ dataTable }: GridDataTableProviderProps) {
  const {
    data = [],
    columns,
    checkBox,
    checkList: controlledCheckList,
    checkHandler: controlledCheckHandler,
    key: rowIdKey,
    groupBy,
    getGroupLabel,
  } = dataTable;

  const [expandedRows, setExpandedRows] = useState<Set<string>>(new Set());
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string>>(new Set());
  const [internalCheckList, setInternalCheckList] = useState<string[]>([]);

  const checkList = controlledCheckList ?? internalCheckList;
  const checkHandler = controlledCheckHandler ?? setInternalCheckList;

  const clearSelection = useCallback(() => {
    checkHandler([]);
  }, [checkHandler]);

  const layout = useMemo(
    () => buildGridColumnLayout(columns as DataTableColumnModel[]),
    [columns],
  );

  const groups = useMemo(
    () =>
      groupTableRows(
        data as Record<string, unknown>[],
        groupBy,
        getGroupLabel
          ? (key, groupRows) => getGroupLabel(key, groupRows)
          : undefined,
      ),
    [data, groupBy, getGroupLabel],
  );

  const toggleRow = (rowId: string) => {
    setExpandedRows((prev) => {
      const next = new Set(prev);
      if (next.has(rowId)) next.delete(rowId);
      else next.add(rowId);
      return next;
    });
  };

  const toggleGroup = (groupKey: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(groupKey)) next.delete(groupKey);
      else next.add(groupKey);
      return next;
    });
  };

  const renderCard = (row: Record<string, unknown>, index: number) => {
    const rowId = getRowId(row, index, rowIdKey ?? "id");
    const isChecked = checkList.includes(rowId);

    return (
      <GridDataTableCard
        key={rowId}
        rowId={rowId}
        row={row}
        layout={layout}
        isExpanded={expandedRows.has(rowId)}
        onToggleExpand={() => toggleRow(rowId)}
        checkBox={checkBox}
        isChecked={isChecked}
        onCheckChange={(checked) =>
          checkHandler(toggleRowSelection(checkList, rowId, checked))
        }
      />
    );
  };

  const chromeDataTable: DataTableModel = {
    ...dataTable,
    checkList,
    selectionCount: checkList.length,
    onClearSelection: checkBox ? clearSelection : undefined,
  };

  return (
    <DataTableChrome dataTable={chromeDataTable}>
      {groups ? (
        <div className="space-y-4 p-1">
          {groups.map((group) => {
            const isCollapsed = collapsedGroups.has(group.key);
            return (
              <GroupSection key={group.key}>
                <GroupTableHeader
                  label={group.label}
                  count={group.rows.length}
                  isCollapsed={isCollapsed}
                  onToggle={() => toggleGroup(group.key)}
                  variant="card"
                />
                {!isCollapsed ? (
                  <GroupBody>
                    <div className="grid grid-cols-[repeat(auto-fill,minmax(300px,1fr))] gap-4">
                      {group.rows.map((row, index) => renderCard(row, index))}
                    </div>
                  </GroupBody>
                ) : null}
              </GroupSection>
            );
          })}
        </div>
      ) : (
        <div className="grid grid-cols-[repeat(auto-fill,minmax(300px,1fr))] gap-4 p-1">
          {data.map((row: Record<string, unknown>, index: number) =>
            renderCard(row, index),
          )}
        </div>
      )}
    </DataTableChrome>
  );
}

export default GridDataTableProvider;
