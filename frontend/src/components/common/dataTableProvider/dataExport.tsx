"use client";
import  { useState } from "react";

import {  Sheet } from "lucide-react";
import * as XLSX from "xlsx";
import ButtonField from "@/components/ui/buttonField";
import Spinner from "../spinner/spinner";
import { useTranslation } from "react-i18next";

interface DataTableColumnModel {
  name?: string;
  label?: any;
  key?: string;
}

function DataExport(props: {
  sourceData: any[];
  title: string;
  columns?: DataTableColumnModel[];
  selectedCols?: DataTableColumnModel[];
}) {
  const { t } = useTranslation();
  const { sourceData, title, columns, selectedCols } = props;
  const [exportLoading, setExportLoading] = useState(false);

  const onGetExporProduct = async () => {
    try {
      setExportLoading(true);
      if (sourceData && Array.isArray(sourceData)) {
        // Filter to only selected columns if available
        let exportData = sourceData;
        
        if (selectedCols && selectedCols.length > 0) {
          const selectedKeys = selectedCols.map(col => col.name || col.key);
          exportData = sourceData.map((row: any) => {
            const filteredRow: any = {};
            selectedKeys.forEach((key) => {
              const k = key ?? '';
              if (row[k] !== undefined) {
                filteredRow[k] = row[k];
              }
            });
            return filteredRow;
          });
          
          // Rename keys to use labels
          const columnMap = new Map();
          columns?.forEach(col => {
            columnMap.set(col.name || col.key, col.label);
          });
          
          exportData = exportData.map((row: any) => {
            const renamedRow: any = {};
            Object.keys(row).forEach(key => {
              const label = columnMap.get(key) || key;
              renamedRow[label] = row[key];
            });
            return renamedRow;
          });
        } else if (columns && columns.length > 0) {
          // Use column labels as keys
          const columnMap = new Map();
          columns.forEach(col => {
            columnMap.set(col.name || col.key, col.label);
          });
          
          exportData = sourceData.map((row: any) => {
            const renamedRow: any = {};
            Object.keys(row).forEach(key => {
              if (columnMap.has(key)) {
                const label = columnMap.get(key);
                renamedRow[label] = row[key];
              }
            });
            return renamedRow;
          });
        }
        
        const workbook = XLSX.utils.book_new();
        const worksheet = XLSX.utils?.json_to_sheet(exportData);
        XLSX.utils.book_append_sheet(workbook, worksheet, "data");
        XLSX.writeFile(workbook, `${title}.xlsx`);
        setExportLoading(false);
      } else {
        setExportLoading(false);
      }
    } catch (error: any) {
      setExportLoading(false);
    }
  };
  return (
    <div className="relative">
      {exportLoading ? (
        <div className="absolute inset-0 z-10 flex items-center justify-center rounded-lg bg-card/80">
          <Spinner size="md" variant="icon" showLabel={false} />
        </div>
      ) : null}
      <div className="relative inline-flex ml-3 text-start   ">
        <ButtonField
          className={`p-1 transition-colors duration-200 bg-secondary/30 text-primary hover:bg-primary/20`}
          onClick={onGetExporProduct}
          icon={<Sheet className="mt-1" size={14} />}
          value={t("Export")}
          disabled={false}
        ></ButtonField>
      </div>
    </div>
  );
}

export default DataExport;
