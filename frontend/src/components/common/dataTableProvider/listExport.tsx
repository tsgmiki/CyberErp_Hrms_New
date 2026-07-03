import {
  Document,
  Page,
  StyleSheet,
  Text,
  View,
  pdf,
} from "@react-pdf/renderer";
import * as XLSX from "xlsx";
import { buildExportRows, buildExportSheetData, type ExportLabelFn } from "./listExportUtils";
import type DataTableColumnModel from "@/models/DataTableColumnModel";

const pdfStyles = StyleSheet.create({
  page: {
    padding: 28,
    fontSize: 8,
    fontFamily: "Helvetica",
  },
  title: {
    fontSize: 12,
    fontWeight: "bold",
    marginBottom: 12,
  },
  meta: {
    fontSize: 8,
    color: "#64748b",
    marginBottom: 10,
  },
  table: {
    width: "100%",
  },
  headerRow: {
    flexDirection: "row",
    backgroundColor: "#f1f5f9",
    borderBottomWidth: 1,
    borderBottomColor: "#cbd5e1",
  },
  row: {
    flexDirection: "row",
    borderBottomWidth: 0.5,
    borderBottomColor: "#e2e8f0",
  },
  headerCell: {
    flex: 1,
    padding: 4,
    fontSize: 7,
    fontWeight: "bold",
  },
  cell: {
    flex: 1,
    padding: 4,
    fontSize: 7,
  },
});

function sanitizeFileName(title: string): string {
  return title.replace(/[<>:"/\\|?*]+/g, "_").trim() || "export";
}

function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

function ListExportPdfDocument({
  title,
  headers,
  rows,
  generatedAt,
}: {
  title: string;
  headers: string[];
  rows: string[][];
  generatedAt: string;
}) {
  return (
    <Document>
      <Page size="A4" orientation="landscape" style={pdfStyles.page}>
        <Text style={pdfStyles.title}>{title}</Text>
        <Text style={pdfStyles.meta}>{generatedAt}</Text>
        <View style={pdfStyles.table}>
          <View style={pdfStyles.headerRow}>
            {headers.map((header) => (
              <Text key={header} style={pdfStyles.headerCell}>
                {header}
              </Text>
            ))}
          </View>
          {rows.map((row, rowIndex) => (
            <View key={`row-${rowIndex}`} style={pdfStyles.row}>
              {row.map((cell, cellIndex) => (
                <Text key={`${rowIndex}-${cellIndex}`} style={pdfStyles.cell}>
                  {cell}
                </Text>
              ))}
            </View>
          ))}
        </View>
      </Page>
    </Document>
  );
}

export async function exportListToExcel(options: {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  labelFor: ExportLabelFn;
}): Promise<void> {
  const sheetData = buildExportSheetData(options.data, options.columns, options.labelFor);
  if (sheetData.length === 0) return;

  const workbook = XLSX.utils.book_new();
  const worksheet = XLSX.utils.json_to_sheet(sheetData);
  XLSX.utils.book_append_sheet(workbook, worksheet, "data");
  XLSX.writeFile(workbook, `${sanitizeFileName(options.title)}.xlsx`);
}

export async function exportListToPdf(options: {
  title: string;
  data: Record<string, unknown>[] | undefined;
  columns: DataTableColumnModel[];
  labelFor: ExportLabelFn;
}): Promise<void> {
  const { headers, rows } = buildExportRows(options.data, options.columns, options.labelFor);
  if (rows.length === 0) return;

  const generatedAt = new Date().toLocaleString();
  const blob = await pdf(
    ListExportPdfDocument({
      title: options.title,
      headers,
      rows,
      generatedAt,
    }),
  ).toBlob();

  downloadBlob(blob, `${sanitizeFileName(options.title)}.pdf`);
}
