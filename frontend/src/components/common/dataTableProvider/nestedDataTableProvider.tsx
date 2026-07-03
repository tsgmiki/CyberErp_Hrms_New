import type { DataTableModel } from "@/models";

import SearchBar from "../searchBar/searchBar";
import Pagination from "../pagination/pagination";
import Loading from "../loader/loader";
import NestedDataTable from "../../ui/nestedDataTable";
import { useState } from "react";
import { patchListParam } from "./listParamUtils";

function NestedDataTableProvider(props: { dataTable: DataTableModel }) {
  const {
    pagination,
    isLoading,
    data,
    columns,
    param,
    key,
    count,
    setParam,
    search,
    GetChildren,
    hideHeader,
  } = props.dataTable;

  const [searchValue, setSearchValue] = useState(param?.searchText ?? "");

  const searchTextHandler = (value: string) => {
    setParam?.(
      patchListParam(param, {
        searchText: value,
      }),
    );
  };
  const paginationHandler = (valObj: { take: number; skip: number }) => {
    setParam?.(
      patchListParam(param, {
        take: valObj.take,
        skip: valObj.skip,
      }),
    );
  };
  const sortHandler = (val: string) => {
    setParam?.(
      patchListParam(param, {
        sortCol: val,
      }),
    );
  };

  return (
    <div key={key}>
      {search == "Visible" && (
        <SearchBar
          value={searchValue}
          onChange={setSearchValue}
          onEnter={() => searchTextHandler(searchValue)}
        />
      )}
      {isLoading && <Loading />}
      <NestedDataTable
        data={data}
        columns={columns}
        key={key}
        count={count}
        sortHandler={sortHandler}
        GetChildren={GetChildren}
        hideHeader={hideHeader}
      ></NestedDataTable>
      {pagination == "Visible" && (
        <Pagination
          recordCount={count as number}
          paginationHandler={paginationHandler}
        />
      )}
    </div>
  );
}
export default NestedDataTableProvider;
