import type { DataTableColumnModel, DataTableModel } from "@/models";
import DataTable from "@/components/ui/dataTable";
import VirtualDataTable from "@/components/ui/virtualDataTable";
import { useCallback, useState } from "react";
import DataTableChrome from "./dataTableChrome";
import { patchListParam } from "./listParamUtils";
import type ParameterModel from "@/models/ParameterModel";

function DataTableProvider(props: { dataTable: DataTableModel }) {
  const {
    isLoading,
    data,
    columns,
    count,
    showSort,
    isVirtual,
    showSummary,
    averageUnitPrice,
    param,
    setParam,
    key: tableKey,
    checkBox,
    checkList: controlledCheckList,
    checkHandler: controlledCheckHandler,
    groupBy,
    getGroupLabel,
    GetChildren,
  } = props.dataTable;

  const [internalCheckList, setInternalCheckList] = useState<string[]>([]);

  const activeColumns = (columns ?? []) as DataTableColumnModel[];
  const checkList = controlledCheckList ?? internalCheckList;
  const checkHandler = controlledCheckHandler ?? setInternalCheckList;

  const clearSelection = useCallback(() => {
    checkHandler([]);
  }, [checkHandler]);

  const sortHandler = (val: string) => {
    if (setParam && param) {
      const currentDir = String(param.dir ?? "").toUpperCase();
      const newDir = param.sortCol === val && currentDir === "ASC" ? "DESC" : "ASC";
      setParam((prev) =>
        patchListParam(prev as ParameterModel | undefined, {
          sortCol: val,
          dir: newDir,
        }),
      );
    }
  };

  const chromeDataTable: DataTableModel = {
    ...props.dataTable,
    checkList,
    selectionCount: checkList.length,
    onClearSelection: checkBox ? clearSelection : undefined,
  };

  return (
    <DataTableChrome dataTable={chromeDataTable}>
      {isVirtual ? (
        <VirtualDataTable
          data={data}
          columns={activeColumns}
          key={tableKey ?? "virtual-table"}
          count={count}
          sortHandler={sortHandler}
          showSummary={showSummary}
          averageUnitPrice={averageUnitPrice}
          param={param}
          isLoading={isLoading}
        />
      ) : (
        <DataTable
          key={tableKey ?? "data-table"}
          data={data}
          columns={activeColumns}
          count={count}
          sortHandler={sortHandler}
          showSort={showSort}
          showSummary={showSummary}
          param={param}
          isLoading={isLoading}
          checkBox={checkBox}
          checkList={checkList}
          checkHandler={checkHandler}
          groupBy={groupBy}
          getGroupLabel={getGroupLabel}
          GetChildren={GetChildren}
          rowIdKey={tableKey}
        />
      )}
    </DataTableChrome>
  );
}

export default DataTableProvider;
