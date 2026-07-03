import { type ReactNode, useCallback, useEffect, useState } from "react";
import type { ParameterModel } from "@/models";
import Pagination from "../pagination/pagination";
import SearchBar from "../searchBar/searchBar";
import { patchListParam } from "./listParamUtils";

export interface DataTablePageProviderProps {
  children: ReactNode;
  param: ParameterModel;
  setParam: React.Dispatch<React.SetStateAction<ParameterModel>>;
  totalItems?: number;
  isLoading?: boolean;
  searchPlaceholder?: string;
  showSearch?: boolean;
  showPagination?: boolean;
}

function DataTablePageProvider({
  children,
  param,
  setParam,
  totalItems = 0,
  isLoading = false,
  searchPlaceholder = "Search...",
  showSearch = true,
  showPagination = true,
}: DataTablePageProviderProps) {
  const [searchInput, setSearchInput] = useState(param.searchText ?? "");

  useEffect(() => {
    setSearchInput(param.searchText ?? "");
  }, [param.searchText]);

  const handleSearchInputChange = useCallback((value: string) => {
    setSearchInput(value);
  }, []);

  const handleSearchSubmit = useCallback(() => {
    setParam((prev) => patchListParam(prev, { searchText: searchInput, skip: 0 }));
  }, [setParam, searchInput]);

  const paginationHandler = useCallback(
    (valObj: { take: number; skip: number }) => {
      setParam((prev) => patchListParam(prev, valObj));
    },
    [setParam],
  );

  return (
    <div className="flex h-full min-h-0 flex-col">
      {showSearch && (
        <div className="mb-3 flex shrink-0 justify-end px-1">
          <div className="w-full sm:w-64 md:w-80">
            <SearchBar
              value={searchInput}
              onChange={handleSearchInputChange}
              onEnter={handleSearchSubmit}
              placeholder={searchPlaceholder}
              disabled={isLoading}
            />
          </div>
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-auto">{children}</div>

      {showPagination && (
        <Pagination
          recordCount={totalItems}
          take={param.take}
          paginationHandler={paginationHandler}
        />
      )}
    </div>
  );
}

export default DataTablePageProvider;
