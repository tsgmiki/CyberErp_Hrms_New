import { Printer, FileSpreadsheet } from "lucide-react";
import ButtonField from "@/components/ui/buttonField";

export type ExportPrintButtonsProps = {
  onExportExcel: () => void;
  onPrint: () => void;
  exportLabel?: string;
  printLabel?: string;
  exportDisabled?: boolean;
  printDisabled?: boolean;
  className?: string;
};

export function ExportPrintButtons({
  onExportExcel,
  onPrint,
  exportLabel = "Export Excel",
  printLabel = "Print",
  exportDisabled = false,
  printDisabled = false,
  className,
}: ExportPrintButtonsProps) {
  return (
    <div className={className ?? "flex gap-2"}>
      <ButtonField
        className="bg-linear-to-r from-green-500 to-green-600 text-white hover:from-green-600 hover:to-green-700 flex items-center gap-2 px-2 py-2 rounded-lg shadow-md hover:shadow-lg transition-all duration-200 transform hover:-translate-y-0.5"
        onClick={onExportExcel}
        icon={<FileSpreadsheet size={18} className="mr-1" />}
        value={exportLabel}
        disabled={exportDisabled}
      />
      <ButtonField
        className="bg-linear-to-r from-blue-500 to-blue-600 text-white hover:from-blue-600 hover:to-blue-700 flex items-center gap-2 px-2 py-2 rounded-lg shadow-md hover:shadow-lg transition-all duration-200 transform hover:-translate-y-0.5"
        onClick={onPrint}
        icon={<Printer size={18} className="mr-1" />}
        value={printLabel}
        disabled={printDisabled}
      />
    </div>
  );
}
