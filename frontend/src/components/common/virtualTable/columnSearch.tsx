import type { ReportCriteriaModel } from "@/models";
import { useSignals } from "@preact/signals-react/runtime";
import DropDownFieldV2 from "@/components/ui/dropDownFieldV2";
import store from "@/store";
import { v4 as uuid } from "uuid";
import { Trash } from "lucide-react";

interface ColumnSearchProps {
  name?: string;
  searchByColumnHandler?: Function;
}
function ColumnSearch({ searchByColumnHandler, name }: ColumnSearchProps) {
  useSignals();

  const report = store.Report.value;
  const criteris =
    report.reportCriterias?.filter((a: ReportCriteriaModel) => a.fieldName == name) || [];

  const search = (reportCriteria?: ReportCriteriaModel) => {
    let report = store.Report.value;

    const reportCriterias =
      report.reportCriterias?.filter(
        (a: ReportCriteriaModel) => a.fieldName != reportCriteria?.fieldName,
      ) || [];

    report = {
      ...report,

      reportCriterias: [
        ...reportCriterias,
        {
          ...reportCriteria,
          id: uuid(),
          fieldName: reportCriteria?.fieldName,
          connector: reportCriteria?.connector || "OR",
          operator: reportCriteria?.operator || "=",
          value: reportCriteria?.value || "",
        },
      ],
    };

    store.Report.value = report;
  };
  const remove = (id: string) => {
    let report = store.Report.value;
    const reportCriterias =
      report.reportCriterias?.filter((a: ReportCriteriaModel) => a.id != id) || [];
    report = {
      ...report,

      reportCriterias: [...reportCriterias],
    };
    store.Report.value = report;
    searchByColumnHandler?.("", "");
  };
  return (
    <DropDownFieldV2
      name={name}
      type={"select"}
      onSearch={(name, text, operator) => {
        searchByColumnHandler?.(name, text);
        search({
          fieldName: name,
          connector: "OR",
          operator: operator,
          value: text,
        });
      }}
      data={criteris?.map((a: ReportCriteriaModel) => {
        return {
          value: a.fieldName,
          remark: (
            <div className="  gap-1 flex justify-between">
              <span className="">
                {a.fieldName + " " + a.operator + " " + a.value}
              </span>
              <button
                onClick={() => {
                  remove(a.id as string);
                }}
              >
                <Trash size={16} />
              </button>
            </div>
          ),
        };
      })}
    />
  );
}

export default ColumnSearch;
