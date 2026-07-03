
import type { DataTableColumnModel } from "@/models";
import CheckBoxList from "@/components/ui/dropDownCheckBoxList";
import { Settings } from "lucide-react";
import { useTranslation } from "react-i18next";

function ColumnFilter(props: {
  record: {};
  selectedCols: DataTableColumnModel[];
  setSelected: Function;
}) {
  const { t } = useTranslation();
  const { record, selectedCols, setSelected } = props;

  const keys = record ? Object.keys(record) : [];

  const columns = keys?.map((item) => {
    return {
      name: item,
      label: item,
      key: item,
      render: (text: any) => (
        <> {!isNaN(+text) ? Number(text).toLocaleString() : text}</>
      ),
    } as DataTableColumnModel;
  });
  const oncolumnSelect = (item: DataTableColumnModel, isAdd: boolean) => {
    if (isAdd) {
      setSelected([...selectedCols, item]);
    } else {
      const records = selectedCols.filter((a: any) => a.name != item.name);
      setSelected(records);
    }
  };
  return (
    <div className="  ">
      <div className="relative inline-flex ml-3 text-start   ">
        <CheckBoxList
          className="  text-[#183B4E] p-0.5  bg-[#F3F3E0] mt-1 hover:text-[#27548A] "
          icon={
            <Settings
            className="mt-1" size={14} 
            />
          }
          disabled={false}
          menu={columns}
          value={t("Columns")}
          selectedMenu={selectedCols}
          onSelect={oncolumnSelect}
        ></CheckBoxList>
      </div>
    </div>
  );
}

export default ColumnFilter;
