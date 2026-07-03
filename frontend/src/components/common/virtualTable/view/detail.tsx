"use client";

import { useRef, useState, useMemo, useCallback } from "react";
import type { ParameterModel, DataTableColumnModel } from "@/models";
import { Printer, ChevronLeft, ChevronRight } from "lucide-react";

function ReportDetail(props: { 
  sourceData: any[]; 
  param?: ParameterModel;
  reportInfo?: {
    columns?: DataTableColumnModel[];
    visibleFields?: string[];
  };
}) {
  const { sourceData, param, reportInfo } = props;

  const contentRef = useRef<HTMLDivElement>(null);
  const [currentPage, setCurrentPage] = useState(1);
  
  const visibleFields = reportInfo?.visibleFields || [];
  const columns = reportInfo?.columns || [];
  
  const columnNames = visibleFields.length > 0 
    ? visibleFields 
    : (sourceData && sourceData.length > 0 ? Object.keys(sourceData[0] as any) : []);
    
  const getColumnLabel = (key: string) => {
    const col = columns.find((c: any) => c.name === key || c.key === key);
    return col?.label || key;
  };

  const reportDate = sourceData && sourceData.length > 0 
    ? new Date().toLocaleDateString() 
    : "";

  const ROWS_PER_PAGE = 20;
  const totalPages = Math.ceil(sourceData.length / ROWS_PER_PAGE);
  
  const paginatedData = useMemo(() => {
    const start = (currentPage - 1) * ROWS_PER_PAGE;
    return sourceData.slice(start, start + ROWS_PER_PAGE);
  }, [sourceData, currentPage]);

  const handlePrint = useCallback(() => {
    const printContent = contentRef.current;
    if (!printContent) return;
    
    const printWindow = window.open("", "_blank");
    if (!printWindow) return;
    
    printWindow.document.write(`
      <!DOCTYPE html>
      <html>
        <head>
          <title>${param?.reportName || "Report"}</title>
          <style>
            body { font-family: Arial, sans-serif; padding: 20px; }
            h1 { font-size: 18px; margin-bottom: 5px; }
            .meta { font-size: 12px; color: #666; margin-bottom: 15px; }
            table { width: 100%; border-collapse: collapse; font-size: 10px; }
            th, td { border: 1px solid #ddd; padding: 6px; text-align: left; }
            th { background-color: #f0f0f0; font-weight: bold; }
            tr:nth-child(even) { background-color: #f9f9f9; }
            .info { display: flex; justify-content: space-between; margin-bottom: 15px; }
            .info-section { background: #f5f5f5; padding: 10px; border-radius: 4px; font-size: 11px; }
          </style>
        </head>
        <body>
          <h1>${param?.reportName || "Report"}</h1>
          <div class="meta">Report Date: ${reportDate}</div>
          <div class="info">
            <div class="info-section">
              <div><strong>From Date:</strong> ${param?.fromDate || "N/A"}</div>
              <div><strong>To Date:</strong> ${param?.toDate || "N/A"}</div>
            </div>
            <div class="info-section">
              <div><strong>Total Records:</strong> ${sourceData.length}</div>
            </div>
          </div>
          <h3>Details:</h3>
          <table>
            <thead>
              <tr>
                ${columnNames.map((key: string) => `<th>${getColumnLabel(key)}</th>`).join("")}
              </tr>
            </thead>
            <tbody>
              ${sourceData.map((item: any) => `
                <tr>
                  ${columnNames.map((key: string) => `<td>${item[key] !== undefined && item[key] !== null ? String(item[key]) : ""}</td>`).join("")}
                </tr>
              `).join("")}
            </tbody>
          </table>
        </body>
      </html>
    `);
    printWindow.document.close();
    printWindow.print();
  }, [sourceData, param, reportDate, columnNames, columns]);

  return (
    <div className="flex flex-col h-full">
      <div className="flex justify-between items-center p-3 border-b border-border bg-primary/20">
        <h2 className="text-lg font-bold text-foreground">
          {param?.reportName || "Report"} - Print Preview
        </h2>
        <div className="flex items-center gap-2">
          <button
            onClick={handlePrint}
            className={`flex items-center gap-2 px-3 py-2 rounded-lg transition-colors bg-primary text-white hover:bg-primary/80`}
          >
            <Printer size={16} />
            Print
          </button>
        </div>
      </div>

      <div className="p-4 overflow-auto flex-1">
        <div ref={contentRef} className="bg-white p-4">
          <div className="text-center mb-6 border-b pb-4">
            <h1 className="text-2xl font-bold text-gray-800">
              {param?.reportName || "Report"}
            </h1>
            <p className="text-sm text-gray-500 mt-1">Report Date: {reportDate}</p>
          </div>

          <div className="flex justify-between mb-4 p-3 bg-secondary rounded text-sm">
            <div>
              <p><span className="font-semibold">From Date:</span> {param?.fromDate || "N/A"}</p>
              <p><span className="font-semibold">To Date:</span> {param?.toDate || "N/A"}</p>
            </div>
            <div className="text-right">
              <p><span className="font-semibold">Total Records:</span> {sourceData.length}</p>
              <p><span className="font-semibold">Page:</span> {currentPage} of {totalPages}</p>
            </div>
          </div>

          <div className="mb-4">
            <h3 className="font-semibold text-gray-700 mb-2">Details:</h3>
            <div className="overflow-x-auto">
              <table className="w-full border-collapse text-sm">
                <thead>
                  <tr className="bg-secondary">
                    {columnNames.map((key: string) => (
                      <th
                        key={key}
                        className="border p-2 text-left font-semibold bg-secondary/50"
                      >
                        {getColumnLabel(key)}
                      </th>
                    ))}
                  </tr>
                </thead>
                <tbody>
                  {paginatedData.map((item: any, idx: number) => (
                    <tr
                      key={idx}
                      className={idx % 2 === 0 ? "bg-white" : "bg-secondary/30"}
                    >
                      {columnNames.map((key: string) => (
                        <td key={key} className="border p-2">
                          {item[key] !== undefined && item[key] !== null 
                            ? String(item[key]) 
                            : ""}
                        </td>
                      ))}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>

      {totalPages > 1 && (
        <div className="flex justify-center items-center gap-4 p-3 border-t border-border bg-primary/10">
          <button
            onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
            disabled={currentPage === 1}
            className={`p-2 rounded transition-colors ${
              currentPage === 1 
                ? "opacity-50 cursor-not-allowed" 
                : "hover:bg-primary/20"
            }`}
          >
            <ChevronLeft size={20} />
          </button>
          <span className="text-sm">
            Page {currentPage} of {totalPages}
          </span>
          <button
            onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
            disabled={currentPage === totalPages}
            className={`p-2 rounded transition-colors ${
              currentPage === totalPages 
                ? "opacity-50 cursor-not-allowed" 
                : "hover:bg-primary/20"
            }`}
          >
            <ChevronRight size={20} />
          </button>
        </div>
      )}

      <style>{`
        @media print {
          body * {
            visibility: hidden;
          }
          .print-container, .print-container * {
            visibility: visible;
          }
          .print-container {
            position: absolute;
            left: 0;
            top: 0;
            width: 100%;
          }
          .no-print {
            display: none !important;
          }
        }
      `}</style>
    </div>
  );
}
export default ReportDetail;
