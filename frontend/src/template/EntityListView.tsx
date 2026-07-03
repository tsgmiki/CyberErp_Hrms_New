"use client";

import type { EntityListViewProps } from "./types";
import { useEntityList } from "./useEntityList";
import { EntityListShell } from "./EntityListShell";

/**
 * All-in-one list screen when columns do not need edit/delete handlers from the parent.
 * For action columns, use {@link useEntityList} + {@link EntityListShell} instead.
 */
export function EntityListView(props: EntityListViewProps) {
  const list = useEntityList({
    queryKey: props.queryKey,
    fetchPage: props.fetchPage,
    deleteById: props.deleteById,
    initialParam: props.initialParam,
  });

  return (
    <EntityListShell
      listKey={props.listKey}
      listLabel={props.listLabel}
      columns={props.columns}
      listFilters={props.listFilters}
      searchBarFilters={props.searchBarFilters}
      header={props.header}
      checkBox={props.checkBox}
      groupBy={props.groupBy}
      getGroupLabel={props.getGroupLabel}
      rowKey={props.rowKey}
      className={props.className}
      {...list}
    />
  );
}

export default EntityListView;
