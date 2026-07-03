import { ChevronDown, RefreshCcw, Search, ArrowUpDown } from "lucide-react";
import DataExport from "../dataTableProvider/dataExport";
import DropDownButton from "@/components/ui/dropDownButton";
import ColumnFilter from "./columnFilter";
import type { DataTableColumnModel } from "@/models";
import View from "./view";
import { useSignals } from "@preact/signals-react/runtime";
import { useState, useCallback } from "react";
import { useTranslation } from "react-i18next";

function ActionBar({
  data = [],
  resetSearchByColumnHandler,
  pageSizeHandler,
  selectedCols,
  setSelected,
  param,
  columns,
  reportInfo,
  sortHandler,
  searchHandler,
}: {
  data?: any[];
  resetSearchByColumnHandler?: Function;
  pageSizeHandler?: Function;
  selectedCols?: any[];
  setSelected?: Function;
  param?: any;
  columns?: any[];
  reportInfo?: any;
  sortHandler?: Function;
  searchHandler?: Function;
}) {
  useSignals();
  const [searchText, setSearchText] = useState(param?.search || "");
  const {t}=useTranslation();
  const handleSearchInput = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchText(e.target.value);
  }, []);

  const handleSearchSubmit = useCallback((e?: any) => {
    if (e?.preventDefault) {
      e.preventDefault();
    }
    
    searchHandler?.(searchText);
  }, [searchText, searchHandler]);

  const handleSort = useCallback((field: string) => {
    sortHandler?.(field);
  }, [sortHandler]);

  const availableColumns = columns?.filter((col: any) => col.sort || col.search) || [];

  return (
    <div className={`p-2 inline-flex gap-2 border-b transition-colors duration-300 bg-secondary/30 border-border`}>
      <button value={t("Reset")}>
        {
          <RefreshCcw
            className={`transition-colors duration-200 text-muted hover:text-foreground`}
            size={14}
            onClick={() => resetSearchByColumnHandler?.()}
          />
        }
      </button>
      <form onSubmit={handleSearchSubmit} className="flex items-center gap-1">
        <input
          type="text"
          value={searchText}
          onChange={handleSearchInput}
          placeholder={t("Search...")}
          className={`px-2 py-1 text-sm rounded border transition-colors duration-200 bg-background border-border text-foreground placeholder:text-muted`}
        />
        <button type="submit">
          <Search size={14} className="text-foreground" />
        </button>
      </form>
      <DropDownButton
        icon={<ArrowUpDown size={16} />}
        value={t("Sort")}
        className={`text-sm rounded-full p-2 transition-colors duration-200 text-foreground hover:bg-primary/20`}
        menu={availableColumns.map((col: any) => ({
          id: col.name,
          label: col.label || col.name,
        }))}
        onClick={(item) => handleSort(item.id)}
      />
      <DataExport 
        sourceData={data as never} 
        title={"data"} 
        columns={columns}
        selectedCols={selectedCols}
      />
      <View sourceData={data as never} param={param} reportInfo={reportInfo} />
      <DropDownButton
        icon={<ChevronDown size={16} />}
        value={t("Page Size")}
        className={`text-sm rounded-full p-2 transition-colors duration-200 text-foreground hover:bg-primary/20`}
        menu={[
          { id: 100, label: "100" },
          { id: 1000, label: "1000" },
          { id: 10000, label: "10000" },
          { id: 100000, label: "100000" },
          { id: 1000000, label: "1000000" },
          { id: 10000000, label: "10000000" },
        ]}
        onClick={(item) => pageSizeHandler?.(item.id)}
      />
      <ColumnFilter
        columns={columns as DataTableColumnModel[]}
        selectedCols={selectedCols as DataTableColumnModel[]}
        setSelected={setSelected as Function}
        reportCategory={param?.reportCategory}
        reportName={param?.reportName}
      />
    </div>
  );
}

export default ActionBar;
