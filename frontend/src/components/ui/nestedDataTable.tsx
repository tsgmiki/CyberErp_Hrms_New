import type { DataTableModel } from "@/models";
import { ArrowUpDown, Circle, Folder } from "lucide-react";
import React from "react";
import { useState } from "react";
import { useTranslation } from "react-i18next";

const NestedDataTable = ({
  data,
  columns,
  sortHandler,
  GetChildren,
  hideHeader,
}: DataTableModel) => {
  const [showDetails, setShowDetails] = useState(
    [] as { id: string; show: boolean }[]
  );
  const { t } = useTranslation();

  const add = (id: string) => {
    const record = showDetails.filter((a) => a.id == id)[0];
    const records = showDetails.filter((a) => a.id != id);
    const newRecord = record
      ? { ...record, show: !record.show }
      : { id: id, show: true };
    setShowDetails([...records, newRecord]);
  };
  return (
    <div className="w-full">
      <table className="w-full min-w-max table-auto text-left">
        <thead className="border-0">
          <tr
            hidden={hideHeader}
            className="border-b border-border bg-card p-4"
          >
            <td
              width={10}
              className="p-3 border border-border"
            >
              <span className="flex justify-between text-sm text-right text-table-header"></span>
            </td>

            {columns?.map((val, colIndex) => (
              <td
                key={`header-${val.name || colIndex}`}
                className={`${
                  val?.responsive === "lg"
                    ? "hidden lg:table-cell"
                    : val?.responsive === "md"
                    ? "hidden md:table-cell"
                    : val?.responsive === "sm"
                    ? "hidden sm:table-cell"
                    : "visible"
                } p-3 border border-border`}
              >
                <span className="flex justify-between text-sm text-right text-table-header">
                  {val.label ? t(val.label) : ""}
                  {val.sort && (
                    <button onClick={() => sortHandler?.(val.name)}>
                      <ArrowUpDown className="text-muted" size={10} />
                    </button>
                  )}
                </span>
              </td>
            ))}
          </tr>
        </thead>

        <tbody>
          {data?.map((item, index) => {
            const baseKey = item.id ?? index;

            return (
              <React.Fragment key={`fragment-${baseKey}`}>
                <tr className="even:bg-secondary inline-flex text-foreground">
                  {item.hasChildren && (
                    <td className="text-sm pt-2.5 text-muted">
                      <button onClick={() => add(item.id)}>
                        <Folder size={18} />
                      </button>
                    </td>
                  )}

                  {columns?.map((col, colIndex) => (
                    <td
                      key={`cell-${baseKey}-${col.name || colIndex}`}
                      onClick={() =>
                        item.hasChildren ? add(item.id) : undefined
                      }
                      className={`${
                  col?.responsive === "lg"
                    ? "hidden md:table-cell"
                    : col?.responsive === "md"
                    ? "hidden sm:table-cell"
                    : "visible"
                } text-sm p-2 inline-flex gap-1`}
                    >
                      {!item.hasChildren && (
                        <Circle size={16} className="pt-2 text-muted" />
                      )}
                      {typeof col.render !== "undefined"
                        ? col.render(item[col.name as string], item)
                        : item[col.name as string]}
                    </td>
                  ))}
                </tr>

                <tr key={`detail-${item.id}`}>
                  <td colSpan={columns?.length}>
                    <div className="pl-4 pt-2">
                      {showDetails.some((a) => a.id === item.id && a.show) && (
                        <div className="border border-border rounded-md overflow-hidden">
                          {GetChildren?.(item)}
                        </div>
                      )}
                    </div>
                  </td>
                </tr>
              </React.Fragment>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};

export default NestedDataTable;
