import { useCallback } from "react";

import type { DataTableColumnModel } from "@/models";
import DropDownField from "@/components/ui/dropDownField";
import { useTranslation } from "react-i18next";

function ColumnSort(props: {
  record: {};
  selectedCols: DataTableColumnModel[];
  setSelected: Function;
}) {
  const { t } = useTranslation();
  const { selectedCols, setSelected } = props;
  const selectHandler = useCallback((_name: string, record: any) => {
    setSelected(record.id);
  }, []);
  return (
    <div className="  ">
      <div className="relative inline-flex ml-3 text-start   ">
        <DropDownField
          className="  text-[#183B4E] p-0.5  bg-[#F3F3E0] mt-1 hover:text-[#27548A] "
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
