import { useCallback } from "react";

import type { DataTableColumnModel } from "@/models";
import DropDownField from "@/components/ui/dropDownField";
import { useSignals } from "@preact/signals-react/runtime";
import { useTranslation } from "react-i18next";

function ColumnSort(props: {
  record: {};
  selectedCols: DataTableColumnModel[];
  setSelected: Function;
}) {
    useSignals();
    const { t } = useTranslation();
  const { selectedCols, setSelected } = props;
  const selectHandler = useCallback((_name: string, record: any) => {
    setSelected(record.id);
  }, []);
  return (
    <div className="  ">
      <div className="relative inline-flex ml-3 text-start   ">
        <DropDownField
          className="  text-[#51545a] p-0.5  bg-slate-50 mt-1 hover:text-slate-400 "
          disabled={false}
          value={t("Columns")}
          data={selectedCols as never}
          onSelect={selectHandler}
          type={"select"}
        ></DropDownField>
      </div>
    </div>
  );
}

export default ColumnSort;
